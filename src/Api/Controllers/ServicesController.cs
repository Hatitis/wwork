using Microsoft.AspNetCore.Mvc;
using SDP.Api.Extensions;
using SDP.Application.Contracts;
using SDP.Application.Dtos;
using SDP.Application.Services;
using SDP.Domain.Entities;

namespace SDP.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/services")]
public sealed class ServicesController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly IServiceNodeRepository _serviceNodeRepository;

    public ServicesController(IProjectRepository projectRepository, IServiceNodeRepository serviceNodeRepository)
    {
        _projectRepository = projectRepository;
        _serviceNodeRepository = serviceNodeRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceNodeDto>>> GetByProject(Guid projectId, CancellationToken cancellationToken)
    {
        if (await _projectRepository.GetByIdAsync(projectId, cancellationToken) is null)
        {
            return NotFound();
        }

        var services = await _serviceNodeRepository.GetByProjectAsync(projectId, cancellationToken);
        return Ok(services.Select(x => x.ToDto()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceNodeDto>> GetById(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        var node = await _serviceNodeRepository.GetByIdAsync(id, cancellationToken);
        if (node is null || node.ProjectId != projectId)
        {
            return NotFound();
        }

        return Ok(node.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<ServiceNodeDto>> Create(Guid projectId, UpsertServiceNodeRequest request, CancellationToken cancellationToken)
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

        var node = new ServiceNode
        {
            ProjectId = projectId,
            Name = request.Name.Trim(),
            Type = request.Type,
            BaseLatencyMs = request.BaseLatencyMs,
            CapacityRps = request.CapacityRps,
            ErrorRatePct = request.ErrorRatePct,
            Responsibility = request.Responsibility.Trim(),
            KeyEndpointsCsv = string.Join(',', request.KeyEndpoints),
            CanvasX = request.CanvasX,
            CanvasY = request.CanvasY,
        };

        await _serviceNodeRepository.AddAsync(node, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { projectId, id = node.Id }, node.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceNodeDto>> Update(Guid projectId, Guid id, UpsertServiceNodeRequest request, CancellationToken cancellationToken)
    {
        var errors = request.Validate();
        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        var node = await _serviceNodeRepository.GetByIdAsync(id, cancellationToken);
        if (node is null || node.ProjectId != projectId)
        {
            return NotFound();
        }

        node.Name = request.Name.Trim();
        node.Type = request.Type;
        node.BaseLatencyMs = request.BaseLatencyMs;
        node.CapacityRps = request.CapacityRps;
        node.ErrorRatePct = request.ErrorRatePct;
        node.Responsibility = request.Responsibility.Trim();
        node.KeyEndpointsCsv = string.Join(',', request.KeyEndpoints);
        node.CanvasX = request.CanvasX;
        node.CanvasY = request.CanvasY;

        await _serviceNodeRepository.UpdateAsync(node, cancellationToken);
        return Ok(node.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        var node = await _serviceNodeRepository.GetByIdAsync(id, cancellationToken);
        if (node is null || node.ProjectId != projectId)
        {
            return NotFound();
        }

        await _serviceNodeRepository.DeleteAsync(node, cancellationToken);
        return NoContent();
    }
}
