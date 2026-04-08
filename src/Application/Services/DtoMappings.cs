using SDP.Application.Dtos;
using SDP.Domain.Entities;

namespace SDP.Application.Services;

public static class DtoMappings
{
    public static ProjectDto ToDto(this Project entity) =>
        new(entity.Id, entity.Name, entity.Description, entity.CreatedAtUtc, entity.UpdatedAtUtc);

    public static ServiceNodeDto ToDto(this ServiceNode entity) =>
        new(
            entity.Id,
            entity.ProjectId,
            entity.Name,
            entity.Type,
            entity.BaseLatencyMs,
            entity.CapacityRps,
            entity.ErrorRatePct,
            entity.Responsibility,
            (entity.KeyEndpointsCsv ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            entity.CanvasX,
            entity.CanvasY);

    public static ServiceLinkDto ToDto(this ServiceLink entity) =>
        new(entity.Id, entity.ProjectId, entity.FromServiceId, entity.ToServiceId, entity.LinkLatencyMs);

    public static TrafficScenarioDto ToDto(this TrafficScenario entity) =>
        new(entity.Id, entity.ProjectId, entity.Name, entity.Description, entity.EntryServiceId, entity.IncomingRps);
}
