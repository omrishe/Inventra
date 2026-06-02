namespace Inventra.Application.Interfaces;

/// <summary>
/// Scoped service that holds the resolved tenant identifiers for the current HTTP request.
/// Populated by TenantMiddleware from validated JWT claims.
/// </summary>
public interface ITenantContext
{
    /// <summary>The tenant (chain) identifier extracted from the JWT claim.</summary>
    Guid ChainId { get; set; }

    /// <summary>
    /// The specific store the user is scoped to.
    /// Null for ChainAdmin users who operate across all stores.
    /// </summary>
    Guid? StoreId { get; set; }
}
