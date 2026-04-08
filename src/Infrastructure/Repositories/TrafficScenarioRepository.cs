using Microsoft.EntityFrameworkCore;
using SDP.Application.Contracts;
using SDP.Domain.Entities;
using SDP.Infrastructure.Persistence;

namespace SDP.Infrastructure.Repositories;

public sealed class TrafficScenarioRepository : ITrafficScenarioRepository
{
    private readonly SdpDbContext _context;

    public TrafficScenarioRepository(SdpDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TrafficScenario>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken)
        => await _context.TrafficScenarios
            .Where(x => x.ProjectId == projectId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task<TrafficScenario?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.TrafficScenarios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<TrafficScenario> AddAsync(TrafficScenario entity, CancellationToken cancellationToken)
    {
        _context.TrafficScenarios.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(TrafficScenario entity, CancellationToken cancellationToken)
    {
        _context.TrafficScenarios.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TrafficScenario entity, CancellationToken cancellationToken)
    {
        _context.TrafficScenarios.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
