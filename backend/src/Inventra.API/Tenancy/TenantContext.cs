using Inventra.Application.Interfaces;

namespace Inventra.API.Tenancy;

/// <summary>
/// Scoped concrete implementation of ITenantContext.
/// Values are set once per request by TenantMiddleware and then read
/// throughout the request lifetime (services, DbContext, etc.).
/// </summary>
public class TenantContext : ITenantContext
{
    public Guid ChainId { get; set; }
    public Guid? StoreId { get; set; }
}
