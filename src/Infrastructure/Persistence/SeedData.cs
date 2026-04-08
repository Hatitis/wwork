using SDP.Domain.Entities;

namespace SDP.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(SdpDbContext context, CancellationToken cancellationToken = default)
    {
        if (context.Projects.Any())
        {
            return;
        }

        var project = new Project
        {
            Name = "Demo E-Commerce",
            Description = "Seeded reference project with API, cache, DB and queue.",
        };

        var api = new ServiceNode
        {
            Project = project,
            Name = "Gateway API",
            Type = ServiceType.Api,
            BaseLatencyMs = 15,
            CapacityRps = 800,
            ErrorRatePct = 0.5,
            Responsibility = "Entry point and orchestration",
            CanvasX = 100,
            CanvasY = 120,
        };

        var cache = new ServiceNode
        {
            Project = project,
            Name = "Product Cache",
            Type = ServiceType.Cache,
            BaseLatencyMs = 4,
            CapacityRps = 1200,
            ErrorRatePct = 0.2,
            Responsibility = "Hot product reads",
            CanvasX = 360,
            CanvasY = 60,
        };

        var db = new ServiceNode
        {
            Project = project,
            Name = "Product DB",
            Type = ServiceType.Database,
            BaseLatencyMs = 30,
            CapacityRps = 350,
            ErrorRatePct = 0.3,
            Responsibility = "Source of truth",
            CanvasX = 360,
            CanvasY = 200,
        };

        var queue = new ServiceNode
        {
            Project = project,
            Name = "Order Queue",
            Type = ServiceType.Queue,
            BaseLatencyMs = 10,
            CapacityRps = 500,
            ErrorRatePct = 0.1,
            Responsibility = "Order async processing",
            CanvasX = 620,
            CanvasY = 120,
        };

        context.ServiceNodes.AddRange(api, cache, db, queue);
        context.ServiceLinks.AddRange(
            new ServiceLink { Project = project, FromService = api, ToService = cache, LinkLatencyMs = 3 },
            new ServiceLink { Project = project, FromService = api, ToService = db, LinkLatencyMs = 7 },
            new ServiceLink { Project = project, FromService = api, ToService = queue, LinkLatencyMs = 5 });

        context.TrafficScenarios.Add(new TrafficScenario
        {
            Project = project,
            Name = "Default Read/Order",
            Description = "Main path for browse + checkout traffic",
            EntryService = api,
            IncomingRps = 250,
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
