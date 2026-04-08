using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SDP.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SdpDbContext>
{
    public SdpDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SdpDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=sdp;Username=sdp;Password=sdp");
        return new SdpDbContext(optionsBuilder.Options);
    }
}
