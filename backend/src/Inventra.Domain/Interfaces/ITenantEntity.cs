namespace Inventra.Domain.Interfaces;

/// <summary>
/// Marker interface for all tenant-scoped entities.
/// Any entity implementing this will be automatically filtered
/// by the active ChainId via EF Core Global Query Filters.
/// </summary>
public interface ITenantEntity
{
    Guid ChainId { get; set; }
}
