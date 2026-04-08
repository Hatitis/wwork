using Microsoft.AspNetCore.Mvc;
using SDP.Api.Extensions;
using SDP.Application.Contracts;
using SDP.Application.Dtos;
using SDP.Application.Services;
using SDP.Domain.Entities;

namespace SDP.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class ScenariosController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly IServiceNodeRepository _serviceNodeRepository;
    private readonly ITrafficScenarioRepository _scenarioRepository;
    private readonly ISimulationService _simulationService;

    public ScenariosController(
        IProjectRepository projectRepository,
        IServiceNodeRepository serviceNodeRepository,
        ITrafficScenarioRepository scenarioRepository,
        ISimulationService simulationService)
    {
        _projectRepository = projectRepository;
        _serviceNodeRepository = serviceNodeRepository;
        _scenarioRepository = scenarioRepository;
        _simulationService = simulationService;
    }

    [HttpGet("projects/{projectId:guid}/scenarios")]
    public async Task<ActionResult<IReadOnlyList<TrafficScenarioDto>>> GetByProject(Guid projectId, CancellationToken cancellationToken)
    {
        if (await _projectRepository.GetByIdAsync(projectId, cancellationToken) is null)
        {
            return NotFound();
        }

        var scenarios = await _scenarioRepository.GetByProjectAsync(projectId, cancellationToken);
        return Ok(scenarios.Select(x => x.ToDto()).ToList());
    }

    [HttpGet("projects/{projectId:guid}/scenarios/{id:guid}")]
    public async Task<ActionResult<TrafficScenarioDto>> GetById(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        var scenario = await _scenarioRepository.GetByIdAsync(id, cancellationToken);
        if (scenario is null || scenario.ProjectId != projectId)
        {
            return NotFound();
        }

        return Ok(scenario.ToDto());
    }

    [HttpPost("projects/{projectId:guid}/scenarios")]
    public async Task<ActionResult<TrafficScenarioDto>> Create(Guid projectId, UpsertTrafficScenarioRequest request, CancellationToken cancellationToken)
    {
        var errors = request.Validate();
        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        if (await _projectRepository.GetByIdAsync(projectId, cancellationToken) is null)
        {
            return NotFound();
        }

        var nodes = await _serviceNodeRepository.GetByProjectAsync(projectId, cancellationToken);
        if (!nodes.Any(x => x.Id == request.EntryServiceId))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["entryServiceId"] = ["Entry service must exist in the project."]
            }));
        }

        var scenario = new TrafficScenario
        {
            ProjectId = projectId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            EntryServiceId = request.EntryServiceId,
            IncomingRps = request.IncomingRps,
        };

        await _scenarioRepository.AddAsync(scenario, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { projectId, id = scenario.Id }, scenario.ToDto());
    }

    [HttpPut("projects/{projectId:guid}/scenarios/{id:guid}")]
    public async Task<ActionResult<TrafficScenarioDto>> Update(Guid projectId, Guid id, UpsertTrafficScenarioRequest request, CancellationToken cancellationToken)
    {
        var errors = request.Validate();
        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        var scenario = await _scenarioRepository.GetByIdAsync(id, cancellationToken);
        if (scenario is null || scenario.ProjectId != projectId)
        {
            return NotFound();
        }

        var nodes = await _serviceNodeRepository.GetByProjectAsync(projectId, cancellationToken);
        if (!nodes.Any(x => x.Id == request.EntryServiceId))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["entryServiceId"] = ["Entry service must exist in the project."]
            }));
        }

        scenario.Name = request.Name.Trim();
        scenario.Description = request.Description.Trim();
        scenario.EntryServiceId = request.EntryServiceId;
        scenario.IncomingRps = request.IncomingRps;

        await _scenarioRepository.UpdateAsync(scenario, cancellationToken);
        return Ok(scenario.ToDto());
    }

    [HttpDelete("projects/{projectId:guid}/scenarios/{id:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        var scenario = await _scenarioRepository.GetByIdAsync(id, cancellationToken);
        if (scenario is null || scenario.ProjectId != projectId)
        {
            return NotFound();
        }

        await _scenarioRepository.DeleteAsync(scenario, cancellationToken);
        return NoContent();
    }

    [HttpPost("scenarios/{id:guid}/simulate")]
    public async Task<ActionResult<SimulationResultDto>> Simulate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _simulationService.SimulateAsync(id, cancellationToken);
        return Ok(result);
    }
}
