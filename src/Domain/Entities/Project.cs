namespace SDP.Domain.Entities;

public sealed class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ServiceNode> ServiceNodes { get; set; } = new List<ServiceNode>();
    public ICollection<ServiceLink> ServiceLinks { get; set; } = new List<ServiceLink>();
    public ICollection<TrafficScenario> TrafficScenarios { get; set; } = new List<TrafficScenario>();
}
