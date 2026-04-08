namespace SDP.Application.Dtos;

public sealed record TrafficScenarioDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string Description,
    Guid EntryServiceId,
    double IncomingRps);

public sealed record UpsertTrafficScenarioRequest(
    string Name,
    string Description,
    Guid EntryServiceId,
    double IncomingRps);
