namespace SDP.Domain.Entities;

public sealed class TrafficScenario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid EntryServiceId { get; set; }
    public ServiceNode EntryService { get; set; } = null!;
    public double IncomingRps { get; set; }
}
