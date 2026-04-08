using SDP.Domain.Entities;

namespace SDP.Application.Contracts;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken);
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Project> AddAsync(Project entity, CancellationToken cancellationToken);
    Task UpdateAsync(Project entity, CancellationToken cancellationToken);
    Task DeleteAsync(Project entity, CancellationToken cancellationToken);
}
