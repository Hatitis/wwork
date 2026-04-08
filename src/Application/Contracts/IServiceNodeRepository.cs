using SDP.Domain.Entities;

namespace SDP.Application.Contracts;

public interface IServiceNodeRepository
{
    Task<IReadOnlyList<ServiceNode>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken);
    Task<ServiceNode?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceNode> AddAsync(ServiceNode entity, CancellationToken cancellationToken);
    Task UpdateAsync(ServiceNode entity, CancellationToken cancellationToken);
    Task DeleteAsync(ServiceNode entity, CancellationToken cancellationToken);
}
