using Microsoft.EntityFrameworkCore;
using SDP.Application.Contracts;
using SDP.Domain.Entities;
using SDP.Infrastructure.Persistence;

namespace SDP.Infrastructure.Repositories;

public sealed class ServiceLinkRepository : IServiceLinkRepository
{
    private readonly SdpDbContext _context;

    public ServiceLinkRepository(SdpDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ServiceLink>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken)
        => await _context.ServiceLinks
            .Where(x => x.ProjectId == projectId)
            .OrderBy(x => x.FromServiceId)
            .ThenBy(x => x.ToServiceId)
            .ToListAsync(cancellationToken);

    public Task<ServiceLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.ServiceLinks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<ServiceLink> AddAsync(ServiceLink entity, CancellationToken cancellationToken)
    {
        _context.ServiceLinks.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(ServiceLink entity, CancellationToken cancellationToken)
    {
        _context.ServiceLinks.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ServiceLink entity, CancellationToken cancellationToken)
    {
        _context.ServiceLinks.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
