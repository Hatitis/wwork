using Microsoft.EntityFrameworkCore;
using SDP.Application.Contracts;
using SDP.Domain.Entities;
using SDP.Infrastructure.Persistence;

namespace SDP.Infrastructure.Repositories;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly SdpDbContext _context;

    public ProjectRepository(SdpDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken)
        => await _context.Projects.OrderBy(x => x.CreatedAtUtc).ToListAsync(cancellationToken);

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Projects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Project> AddAsync(Project entity, CancellationToken cancellationToken)
    {
        _context.Projects.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Project entity, CancellationToken cancellationToken)
    {
        entity.UpdatedAtUtc = DateTime.UtcNow;
        _context.Projects.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Project entity, CancellationToken cancellationToken)
    {
        _context.Projects.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
