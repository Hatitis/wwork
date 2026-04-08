using SDP.Application.Contracts;
using SDP.Application.Dtos;
using SDP.Application.Simulation;
using SDP.Domain.Entities;

namespace SDP.Application.Services;

public sealed class SimulationService : ISimulationService
{
    private readonly ITrafficScenarioRepository _scenarioRepository;
    private readonly IServiceNodeRepository _serviceNodeRepository;
    private readonly IServiceLinkRepository _serviceLinkRepository;

    public SimulationService(
        ITrafficScenarioRepository scenarioRepository,
        IServiceNodeRepository serviceNodeRepository,
        IServiceLinkRepository serviceLinkRepository)
    {
        _scenarioRepository = scenarioRepository;
        _serviceNodeRepository = serviceNodeRepository;
        _serviceLinkRepository = serviceLinkRepository;
    }

    public async Task<SimulationResultDto> SimulateAsync(Guid scenarioId, CancellationToken cancellationToken)
    {
        var scenario = await _scenarioRepository.GetByIdAsync(scenarioId, cancellationToken)
            ?? throw new KeyNotFoundException($"Scenario {scenarioId} was not found.");

        var nodes = (await _serviceNodeRepository.GetByProjectAsync(scenario.ProjectId, cancellationToken))
            .OrderBy(x => x.Id)
            .ToList();

        var links = (await _serviceLinkRepository.GetByProjectAsync(scenario.ProjectId, cancellationToken))
            .OrderBy(x => x.FromServiceId)
            .ThenBy(x => x.ToServiceId)
            .ToList();

        var nodeMap = nodes.ToDictionary(x => x.Id);
        if (!nodeMap.ContainsKey(scenario.EntryServiceId))
        {
            throw new SimulationException("Scenario entry service does not exist in this project.");
        }

        var adjacency = links
            .GroupBy(x => x.FromServiceId)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.ToServiceId).ToList());

        var reachable = ComputeReachable(adjacency, scenario.EntryServiceId);
        var warnings = new List<string>();
        var orphanCount = nodes.Count - reachable.Count;
        if (orphanCount > 0)
        {
            warnings.Add($"{orphanCount} service nodes are not reachable from the scenario entry point.");
        }

        var topological = TopologicalOrderOrThrow(reachable, adjacency);

        var incoming = reachable.ToDictionary(x => x, _ => 0d);
        incoming[scenario.EntryServiceId] = scenario.IncomingRps;

        foreach (var serviceId in topological)
        {
            if (!adjacency.TryGetValue(serviceId, out var outgoing) || outgoing.Count == 0)
            {
                continue;
            }

            var perEdge = incoming[serviceId] / outgoing.Count;
            foreach (var edge in outgoing.Where(x => reachable.Contains(x.ToServiceId)))
            {
                incoming[edge.ToServiceId] += perEdge;
            }
        }

        var serviceResults = topological
            .Select(id =>
            {
                var node = nodeMap[id];
                var load = incoming[id];
                var utilization = node.CapacityRps <= 0 ? double.PositiveInfinity : load / node.CapacityRps;
                var avgOutgoingLinkLatency = adjacency.TryGetValue(id, out var outgoing)
                    ? outgoing.Select(x => x.LinkLatencyMs).DefaultIfEmpty(0).Average()
                    : 0;

                return new ServiceSimulationResultDto(
                    id,
                    node.Name,
                    Math.Round(load, 4),
                    Math.Round(utilization, 4),
                    utilization > 1.0,
                    Math.Round(node.BaseLatencyMs + avgOutgoingLinkLatency, 4));
            })
            .ToList();

        var bottlenecks = serviceResults
            .Where(x => x.IsBottleneck)
            .Select(x => x.ServiceId)
            .ToHashSet();

        var pathResults = BuildPathResults(
            scenario.EntryServiceId,
            adjacency,
            nodeMap,
            reachable,
            bottlenecks);

        return new SimulationResultDto(serviceResults, pathResults, warnings);
    }

    private static HashSet<Guid> ComputeReachable(
        IReadOnlyDictionary<Guid, List<ServiceLink>> adjacency,
        Guid entryServiceId)
    {
        var reachable = new HashSet<Guid>();
        var stack = new Stack<Guid>();
        stack.Push(entryServiceId);

        while (stack.TryPop(out var nodeId))
        {
            if (!reachable.Add(nodeId))
            {
                continue;
            }

            if (!adjacency.TryGetValue(nodeId, out var edges))
            {
                continue;
            }

            for (var i = edges.Count - 1; i >= 0; i--)
            {
                stack.Push(edges[i].ToServiceId);
            }
        }

        return reachable;
    }

    private static List<Guid> TopologicalOrderOrThrow(
        HashSet<Guid> reachable,
        IReadOnlyDictionary<Guid, List<ServiceLink>> adjacency)
    {
        var inDegree = reachable.ToDictionary(x => x, _ => 0);

        foreach (var from in reachable)
        {
            if (!adjacency.TryGetValue(from, out var edges))
            {
                continue;
            }

            foreach (var edge in edges)
            {
                if (reachable.Contains(edge.ToServiceId))
                {
                    inDegree[edge.ToServiceId] += 1;
                }
            }
        }

        var queue = new Queue<Guid>(inDegree.Where(x => x.Value == 0).Select(x => x.Key).OrderBy(x => x));
        var ordered = new List<Guid>(reachable.Count);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            ordered.Add(current);

            if (!adjacency.TryGetValue(current, out var edges))
            {
                continue;
            }

            foreach (var edge in edges.Where(x => reachable.Contains(x.ToServiceId)).OrderBy(x => x.ToServiceId))
            {
                inDegree[edge.ToServiceId] -= 1;
                if (inDegree[edge.ToServiceId] == 0)
                {
                    queue.Enqueue(edge.ToServiceId);
                }
            }
        }

        if (ordered.Count != reachable.Count)
        {
            throw new SimulationException("Cycle detected in the reachable service graph. v0.3 fail-fast policy blocks simulation.");
        }

        return ordered;
    }

    private static List<PathSimulationResultDto> BuildPathResults(
        Guid entryServiceId,
        IReadOnlyDictionary<Guid, List<ServiceLink>> adjacency,
        IReadOnlyDictionary<Guid, ServiceNode> nodeMap,
        HashSet<Guid> reachable,
        HashSet<Guid> bottlenecks)
    {
        var results = new List<PathSimulationResultDto>();
        var path = new List<Guid>();
        DepthFirstPaths(entryServiceId, adjacency, nodeMap, reachable, bottlenecks, path, 0, results);
        return results;
    }

    private static void DepthFirstPaths(
        Guid current,
        IReadOnlyDictionary<Guid, List<ServiceLink>> adjacency,
        IReadOnlyDictionary<Guid, ServiceNode> nodeMap,
        HashSet<Guid> reachable,
        HashSet<Guid> bottlenecks,
        List<Guid> currentPath,
        double currentLatency,
        List<PathSimulationResultDto> output)
    {
        currentPath.Add(current);
        currentLatency += nodeMap[current].BaseLatencyMs;

        var outgoing = adjacency.TryGetValue(current, out var links)
            ? links.Where(x => reachable.Contains(x.ToServiceId)).OrderBy(x => x.ToServiceId).ToList()
            : [];

        if (outgoing.Count == 0)
        {
            var pathSnapshot = currentPath.ToList();
            var pathNames = pathSnapshot.Select(x => nodeMap[x].Name).ToList();
            var bottleneckIds = pathSnapshot.Where(bottlenecks.Contains).ToList();
            output.Add(new PathSimulationResultDto(pathSnapshot, pathNames, Math.Round(currentLatency, 4), bottleneckIds));
            currentPath.RemoveAt(currentPath.Count - 1);
            return;
        }

        foreach (var edge in outgoing)
        {
            DepthFirstPaths(
                edge.ToServiceId,
                adjacency,
                nodeMap,
                reachable,
                bottlenecks,
                currentPath,
                currentLatency + edge.LinkLatencyMs,
                output);
        }

        currentPath.RemoveAt(currentPath.Count - 1);
    }
}
