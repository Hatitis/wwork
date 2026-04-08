namespace SDP.Application.Dtos;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record UpsertProjectRequest(string Name, string Description);
