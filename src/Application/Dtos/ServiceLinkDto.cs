namespace SDP.Application.Dtos;

public sealed record ServiceLinkDto(
    Guid Id,
    Guid ProjectId,
    Guid FromServiceId,
    Guid ToServiceId,
    int LinkLatencyMs);

public sealed record UpsertServiceLinkRequest(
    Guid FromServiceId,
    Guid ToServiceId,
    int LinkLatencyMs);
