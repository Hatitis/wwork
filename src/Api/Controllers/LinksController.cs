using Microsoft.AspNetCore.Mvc;
using SDP.Api.Extensions;
using SDP.Application.Contracts;
using SDP.Application.Dtos;
using SDP.Application.Services;
using SDP.Domain.Entities;

namespace SDP.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/links")]
public sealed class LinksController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly IServiceNodeRepository _serviceNodeRepository;
    private readonly IServiceLinkRepository _serviceLinkRepository;

    public LinksController(
        IProjectRepository projectRepository,
        IServiceNodeRepository serviceNodeRepository,
        IServiceLinkRepository serviceLinkRepository)
    {
        _projectRepository = projectRepository;
        _serviceNodeRepository = serviceNodeRepository;
        _serviceLinkRepository = serviceLinkRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceLinkDto>>> GetByProject(Guid projectId, CancellationToken cancellationToken)
    {
        if (await _projectRepository.GetByIdAsync(projectId, cancellationToken) is null)
        {
            return NotFound();
        }

        var links = await _serviceLinkRepository.GetByProjectAsync(projectId, cancellationToken);
        return Ok(links.Select(x => x.ToDto()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceLinkDto>> GetById(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        var link = await _serviceLinkRepository.GetByIdAsync(id, cancellationToken);
        if (link is null || link.ProjectId != projectId)
        {
            return NotFound();
        }

        return Ok(link.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<ServiceLinkDto>> Create(Guid projectId, UpsertServiceLinkRequest request, CancellationToken cancellationToken)
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
        var nodeIds = nodes.Select(x => x.Id).ToHashSet();
        if (!nodeIds.Contains(request.FromServiceId) || !nodeIds.Contains(request.ToServiceId))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["fromServiceId"] = ["Both services must exist in the same project."]
            }));
        }

        var link = new ServiceLink
        {
            ProjectId = projectId,
            FromServiceId = request.FromServiceId,
            ToServiceId = request.ToServiceId,
            LinkLatencyMs = request.LinkLatencyMs,
        };

        await _serviceLinkRepository.AddAsync(link, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { projectId, id = link.Id }, link.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceLinkDto>> Update(Guid projectId, Guid id, UpsertServiceLinkRequest request, CancellationToken cancellationToken)
    {
        var errors = request.Validate();
        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        var link = await _serviceLinkRepository.GetByIdAsync(id, cancellationToken);
        if (link is null || link.ProjectId != projectId)
        {
            return NotFound();
        }

        var nodes = await _serviceNodeRepository.GetByProjectAsync(projectId, cancellationToken);
        var nodeIds = nodes.Select(x => x.Id).ToHashSet();
        if (!nodeIds.Contains(request.FromServiceId) || !nodeIds.Contains(request.ToServiceId))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["fromServiceId"] = ["Both services must exist in the same project."]
            }));
        }

        link.FromServiceId = request.FromServiceId;
        link.ToServiceId = request.ToServiceId;
        link.LinkLatencyMs = request.LinkLatencyMs;

        await _serviceLinkRepository.UpdateAsync(link, cancellationToken);
        return Ok(link.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        var link = await _serviceLinkRepository.GetByIdAsync(id, cancellationToken);
        if (link is null || link.ProjectId != projectId)
        {
            return NotFound();
        }

        await _serviceLinkRepository.DeleteAsync(link, cancellationToken);
        return NoContent();
    }
}
