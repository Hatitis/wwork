namespace SDP.Application.Dtos;

public sealed record SimulationResultDto(
    IReadOnlyList<ServiceSimulationResultDto> ServiceResults,
    IReadOnlyList<PathSimulationResultDto> PathResults,
    IReadOnlyList<string> Warnings);

public sealed record ServiceSimulationResultDto(
    Guid ServiceId,
    string ServiceName,
    double IncomingRps,
    double Utilization,
    bool IsBottleneck,
    double EstimatedLatencyMs);

public sealed record PathSimulationResultDto(
    IReadOnlyList<Guid> Path,
    IReadOnlyList<string> PathNames,
    double TotalLatencyMs,
    IReadOnlyList<Guid> BottleneckServices);
