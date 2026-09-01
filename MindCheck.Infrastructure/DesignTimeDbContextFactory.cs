using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MindCheck.Infrastructure;

// Used only by `dotnet ef migrations add` when there is no startup project
// wired up yet; never used at runtime.
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MindCheckDbContext>
{
    public MindCheckDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MindCheckDbContext>();
        optionsBuilder
            .UseNpgsql("Host=localhost;Database=mindcheck;Username=postgres;Password=postgres")
            .UseSnakeCaseNamingConvention();

        return new MindCheckDbContext(optionsBuilder.Options);
    }
}
