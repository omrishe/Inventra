using Inventra.Domain.Interfaces;

namespace Inventra.Domain.Entities;

/// <summary>
/// A physical store location belonging to a Chain (tenant).
/// Implements ITenantEntity so Global Query Filters scope it to the active ChainId.
/// </summary>
public class Store : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid ChainId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Chain Chain { get; set; } = null!;
}
