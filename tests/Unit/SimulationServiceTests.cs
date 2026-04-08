using FluentAssertions;
using SDP.Application.Contracts;
using SDP.Application.Services;
using SDP.Application.Simulation;
using SDP.Domain.Entities;

namespace SDP.UnitTests;

public sealed class SimulationServiceTests
{
    [Fact]
    public async Task SimulateAsync_SingleNode_ComputesUtilizationAndLatency()
    {
        var projectId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var scenarioId = Guid.NewGuid();

        var service = new ServiceNode
        {
            Id = serviceId,
            ProjectId = projectId,
            Name = "API",
            BaseLatencyMs = 20,
            CapacityRps = 100,
        };

        var scenario = new TrafficScenario
        {
            Id = scenarioId,
            ProjectId = projectId,
            EntryServiceId = serviceId,
            IncomingRps = 50,
            Name = "default"
        };

        var sut = BuildService([service], [], [scenario]);

        var result = await sut.SimulateAsync(scenarioId, CancellationToken.None);

        result.ServiceResults.Should().ContainSingle();
        var item = result.ServiceResults.Single();
        item.IncomingRps.Should().Be(50);
        item.Utilization.Should().Be(0.5);
        item.IsBottleneck.Should().BeFalse();
        item.EstimatedLatencyMs.Should().Be(20);
    }

    [Fact]
    public async Task SimulateAsync_FanOut_PropagatesLoadEvenly()
    {
        var projectId = Guid.NewGuid();
        var a = Node(projectId, "A", 1000);
        var b = Node(projectId, "B", 100);
        var c = Node(projectId, "C", 100);

        var links = new[]
        {
            new ServiceLink { ProjectId = projectId, FromServiceId = a.Id, ToServiceId = b.Id, LinkLatencyMs = 2 },
            new ServiceLink { ProjectId = projectId, FromServiceId = a.Id, ToServiceId = c.Id, LinkLatencyMs = 2 },
        };

        var scenario = new TrafficScenario { Id = Guid.NewGuid(), ProjectId = projectId, EntryServiceId = a.Id, IncomingRps = 80, Name = "s" };

        var sut = BuildService([a, b, c], links, [scenario]);

        var result = await sut.SimulateAsync(scenario.Id, CancellationToken.None);

        result.ServiceResults.Single(x => x.ServiceId == b.Id).IncomingRps.Should().Be(40);
        result.ServiceResults.Single(x => x.ServiceId == c.Id).IncomingRps.Should().Be(40);
    }

    [Fact]
    public async Task SimulateAsync_SharedDependency_AccumulatesLoad()
    {
        var projectId = Guid.NewGuid();
        var a = Node(projectId, "A", 1000);
        var b = Node(projectId, "B", 100);
        var c = Node(projectId, "C", 100);
        var d = Node(projectId, "D", 50);

        var links = new[]
        {
            new ServiceLink { ProjectId = projectId, FromServiceId = a.Id, ToServiceId = b.Id, LinkLatencyMs = 2 },
            new ServiceLink { ProjectId = projectId, FromServiceId = a.Id, ToServiceId = c.Id, LinkLatencyMs = 2 },
            new ServiceLink { ProjectId = projectId, FromServiceId = b.Id, ToServiceId = d.Id, LinkLatencyMs = 2 },
            new ServiceLink { ProjectId = projectId, FromServiceId = c.Id, ToServiceId = d.Id, LinkLatencyMs = 2 },
        };

        var scenario = new TrafficScenario { Id = Guid.NewGuid(), ProjectId = projectId, EntryServiceId = a.Id, IncomingRps = 120, Name = "s" };

        var sut = BuildService([a, b, c, d], links, [scenario]);
        var result = await sut.SimulateAsync(scenario.Id, CancellationToken.None);

        result.ServiceResults.Single(x => x.ServiceId == d.Id).IncomingRps.Should().Be(120);
        result.ServiceResults.Single(x => x.ServiceId == d.Id).IsBottleneck.Should().BeTrue();
    }

    [Fact]
    public async Task SimulateAsync_ProducesDeterministicOutputOrdering()
    {
        var projectId = Guid.NewGuid();
        var a = Node(projectId, "A", 1000);
        var b = Node(projectId, "B", 1000);
        var c = Node(projectId, "C", 1000);

        var links = new[]
        {
            new ServiceLink { ProjectId = projectId, FromServiceId = a.Id, ToServiceId = c.Id, LinkLatencyMs = 1 },
            new ServiceLink { ProjectId = projectId, FromServiceId = a.Id, ToServiceId = b.Id, LinkLatencyMs = 1 },
        };

        var scenario = new TrafficScenario { Id = Guid.NewGuid(), ProjectId = projectId, EntryServiceId = a.Id, IncomingRps = 30, Name = "s" };
        var sut = BuildService([a, b, c], links, [scenario]);

        var first = await sut.SimulateAsync(scenario.Id, CancellationToken.None);
        var second = await sut.SimulateAsync(scenario.Id, CancellationToken.None);

        first.ServiceResults.Select(x => x.ServiceId).Should().Equal(second.ServiceResults.Select(x => x.ServiceId));
        first.PathResults.Select(x => string.Join(',', x.Path)).Should().Equal(second.PathResults.Select(x => string.Join(',', x.Path)));
    }

    [Fact]
    public async Task SimulateAsync_OnCycle_ThrowsSimulationException()
    {
        var projectId = Guid.NewGuid();
        var a = Node(projectId, "A", 100);
        var b = Node(projectId, "B", 100);

        var links = new[]
        {
            new ServiceLink { ProjectId = projectId, FromServiceId = a.Id, ToServiceId = b.Id, LinkLatencyMs = 1 },
            new ServiceLink { ProjectId = projectId, FromServiceId = b.Id, ToServiceId = a.Id, LinkLatencyMs = 1 },
        };

        var scenario = new TrafficScenario { Id = Guid.NewGuid(), ProjectId = projectId, EntryServiceId = a.Id, IncomingRps = 30, Name = "s" };
        var sut = BuildService([a, b], links, [scenario]);

        var action = () => sut.SimulateAsync(scenario.Id, CancellationToken.None);
        await action.Should().ThrowAsync<SimulationException>()
            .WithMessage("*Cycle detected*");
    }

    private static ServiceNode Node(Guid projectId, string name, double capacity)
        => new() { Id = Guid.NewGuid(), ProjectId = projectId, Name = name, CapacityRps = capacity, BaseLatencyMs = 10 };

    private static SimulationService BuildService(
        IReadOnlyList<ServiceNode> nodes,
        IReadOnlyList<ServiceLink> links,
        IReadOnlyList<TrafficScenario> scenarios)
    {
        var nodeRepo = new FakeServiceNodeRepository(nodes);
        var linkRepo = new FakeServiceLinkRepository(links);
        var scenarioRepo = new FakeTrafficScenarioRepository(scenarios);
        return new SimulationService(scenarioRepo, nodeRepo, linkRepo);
    }

    private sealed class FakeServiceNodeRepository : IServiceNodeRepository
    {
        private readonly List<ServiceNode> _items;

        public FakeServiceNodeRepository(IEnumerable<ServiceNode> items) => _items = items.ToList();

        public Task<IReadOnlyList<ServiceNode>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ServiceNode>>(_items.Where(x => x.ProjectId == projectId).ToList());

        public Task<ServiceNode?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

        public Task<ServiceNode> AddAsync(ServiceNode entity, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task UpdateAsync(ServiceNode entity, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task DeleteAsync(ServiceNode entity, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    private sealed class FakeServiceLinkRepository : IServiceLinkRepository
    {
        private readonly List<ServiceLink> _items;

        public FakeServiceLinkRepository(IEnumerable<ServiceLink> items) => _items = items.ToList();

        public Task<IReadOnlyList<ServiceLink>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ServiceLink>>(_items.Where(x => x.ProjectId == projectId).ToList());

        public Task<ServiceLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

        public Task<ServiceLink> AddAsync(ServiceLink entity, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task UpdateAsync(ServiceLink entity, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task DeleteAsync(ServiceLink entity, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    private sealed class FakeTrafficScenarioRepository : ITrafficScenarioRepository
    {
        private readonly List<TrafficScenario> _items;

        public FakeTrafficScenarioRepository(IEnumerable<TrafficScenario> items) => _items = items.ToList();

        public Task<IReadOnlyList<TrafficScenario>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<TrafficScenario>>(_items.Where(x => x.ProjectId == projectId).ToList());

        public Task<TrafficScenario?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

        public Task<TrafficScenario> AddAsync(TrafficScenario entity, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task UpdateAsync(TrafficScenario entity, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task DeleteAsync(TrafficScenario entity, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
