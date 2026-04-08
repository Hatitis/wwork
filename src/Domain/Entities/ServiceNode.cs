namespace SDP.Domain.Entities;

public sealed class ServiceNode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public ServiceType Type { get; set; }
    public int BaseLatencyMs { get; set; }
    public double CapacityRps { get; set; }
    public double ErrorRatePct { get; set; }
    public string Responsibility { get; set; } = string.Empty;
    public string? KeyEndpointsCsv { get; set; }
    public double CanvasX { get; set; }
    public double CanvasY { get; set; }

    public ICollection<ServiceLink> OutgoingLinks { get; set; } = new List<ServiceLink>();
    public ICollection<ServiceLink> IncomingLinks { get; set; } = new List<ServiceLink>();
}
