using SDP.Domain.Entities;

namespace SDP.Application.Contracts;

public interface ITrafficScenarioRepository
{
    Task<IReadOnlyList<TrafficScenario>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken);
    Task<TrafficScenario?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TrafficScenario> AddAsync(TrafficScenario entity, CancellationToken cancellationToken);
    Task UpdateAsync(TrafficScenario entity, CancellationToken cancellationToken);
    Task DeleteAsync(TrafficScenario entity, CancellationToken cancellationToken);
}
