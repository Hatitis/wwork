using SDP.Application.Dtos;

namespace SDP.Application.Contracts;

public interface ISimulationService
{
    Task<SimulationResultDto> SimulateAsync(Guid scenarioId, CancellationToken cancellationToken);
}
