using Microsoft.EntityFrameworkCore;
using SDP.Application.Contracts;
using SDP.Domain.Entities;
using SDP.Infrastructure.Persistence;

namespace SDP.Infrastructure.Repositories;

public sealed class ServiceNodeRepository : IServiceNodeRepository
{
    private readonly SdpDbContext _context;

    public ServiceNodeRepository(SdpDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ServiceNode>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken)
        => await _context.ServiceNodes
            .Where(x => x.ProjectId == projectId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task<ServiceNode?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.ServiceNodes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<ServiceNode> AddAsync(ServiceNode entity, CancellationToken cancellationToken)
    {
        _context.ServiceNodes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(ServiceNode entity, CancellationToken cancellationToken)
    {
        _context.ServiceNodes.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ServiceNode entity, CancellationToken cancellationToken)
    {
        _context.ServiceNodes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
