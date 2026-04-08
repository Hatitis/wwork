using SDP.Domain.Entities;

namespace SDP.Application.Contracts;

public interface IServiceLinkRepository
{
    Task<IReadOnlyList<ServiceLink>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken);
    Task<ServiceLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceLink> AddAsync(ServiceLink entity, CancellationToken cancellationToken);
    Task UpdateAsync(ServiceLink entity, CancellationToken cancellationToken);
    Task DeleteAsync(ServiceLink entity, CancellationToken cancellationToken);
}
