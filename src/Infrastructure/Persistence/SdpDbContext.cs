using Microsoft.EntityFrameworkCore;
using SDP.Domain.Entities;

namespace SDP.Infrastructure.Persistence;

public sealed class SdpDbContext : DbContext
{
    public SdpDbContext(DbContextOptions<SdpDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ServiceNode> ServiceNodes => Set<ServiceNode>();
    public DbSet<ServiceLink> ServiceLinks => Set<ServiceLink>();
    public DbSet<TrafficScenario> TrafficScenarios => Set<TrafficScenario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1500);
            entity.HasMany(x => x.ServiceNodes)
                .WithOne(x => x.Project)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.ServiceLinks)
                .WithOne(x => x.Project)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.TrafficScenarios)
                .WithOne(x => x.Project)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceNode>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Responsibility).HasMaxLength(1500);
            entity.Property(x => x.KeyEndpointsCsv).HasMaxLength(2000);
            entity.HasIndex(x => new { x.ProjectId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<ServiceLink>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ProjectId, x.FromServiceId, x.ToServiceId }).IsUnique();

            entity.HasOne(x => x.FromService)
                .WithMany(x => x.OutgoingLinks)
                .HasForeignKey(x => x.FromServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ToService)
                .WithMany(x => x.IncomingLinks)
                .HasForeignKey(x => x.ToServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TrafficScenario>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1500);
            entity.HasOne(x => x.EntryService)
                .WithMany()
                .HasForeignKey(x => x.EntryServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
