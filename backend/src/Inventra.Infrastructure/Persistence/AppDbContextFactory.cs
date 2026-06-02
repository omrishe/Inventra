using Inventra.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Inventra.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by `dotnet ef migrations` tooling.
/// At design time, there is no HTTP request, so we supply an empty ITenantContext.
/// The Global Query Filters will be built with Guid.Empty — safe for schema generation.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Connection string only used for migration scaffolding — not for production.
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=inventra_db;Username=postgres;Password=postgres");

        return new AppDbContext(optionsBuilder.Options, new NullTenantContext());
    }
}

/// <summary>Empty tenant context for design-time tooling only.</summary>
file sealed class NullTenantContext : ITenantContext
{
    public Guid ChainId { get; set; } = Guid.Empty;
    public Guid? StoreId { get; set; } = null;
}
