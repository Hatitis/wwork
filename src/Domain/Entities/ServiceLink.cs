namespace SDP.Domain.Entities;

public sealed class ServiceLink
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid FromServiceId { get; set; }
    public ServiceNode FromService { get; set; } = null!;
    public Guid ToServiceId { get; set; }
    public ServiceNode ToService { get; set; } = null!;

    public int LinkLatencyMs { get; set; }
}
