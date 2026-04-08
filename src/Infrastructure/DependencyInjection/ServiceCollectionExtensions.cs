using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDP.Application.Contracts;
using SDP.Application.Services;
using SDP.Infrastructure.Persistence;
using SDP.Infrastructure.Repositories;

namespace SDP.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SdpDb")
            ?? throw new InvalidOperationException("Connection string 'SdpDb' is missing.");

        services.AddDbContext<SdpDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IServiceNodeRepository, ServiceNodeRepository>();
        services.AddScoped<IServiceLinkRepository, ServiceLinkRepository>();
        services.AddScoped<ITrafficScenarioRepository, TrafficScenarioRepository>();
        services.AddScoped<ISimulationService, SimulationService>();

        return services;
    }
}
