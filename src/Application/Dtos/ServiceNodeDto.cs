using SDP.Domain.Entities;

namespace SDP.Application.Dtos;

public sealed record ServiceNodeDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    ServiceType Type,
    int BaseLatencyMs,
    double CapacityRps,
    double ErrorRatePct,
    string Responsibility,
    IReadOnlyList<string> KeyEndpoints,
    double CanvasX,
    double CanvasY);

public sealed record UpsertServiceNodeRequest(
    string Name,
    ServiceType Type,
    int BaseLatencyMs,
    double CapacityRps,
    double ErrorRatePct,
    string Responsibility,
    IReadOnlyList<string> KeyEndpoints,
    double CanvasX,
    double CanvasY);
