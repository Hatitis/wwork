using Microsoft.AspNetCore.Mvc;
using SDP.Api.Extensions;
using SDP.Application.Contracts;
using SDP.Application.Dtos;
using SDP.Application.Services;
using SDP.Domain.Entities;

namespace SDP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;

    public ProjectsController(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAll(CancellationToken cancellationToken)
    {
        var projects = await _projectRepository.GetAllAsync(cancellationToken);
        return Ok(projects.Select(x => x.ToDto()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project is null)
        {
            return NotFound();
        }

        return Ok(project.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(UpsertProjectRequest request, CancellationToken cancellationToken)
    {
        var errors = request.Validate();
        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        var project = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
        };

        await _projectRepository.AddAsync(project, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, UpsertProjectRequest request, CancellationToken cancellationToken)
    {
        var errors = request.Validate();
        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project is null)
        {
            return NotFound();
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description.Trim();
        await _projectRepository.UpdateAsync(project, cancellationToken);

        return Ok(project.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project is null)
        {
            return NotFound();
        }

        await _projectRepository.DeleteAsync(project, cancellationToken);
        return NoContent();
    }
}
