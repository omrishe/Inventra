using Inventra.Domain.Enums;

namespace Inventra.Domain.Entities;

/// <summary>
/// The top-level tenant aggregate root.
/// Every other tenant-scoped entity references this via ChainId.
/// </summary>
public class Chain
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PlanType PlanType { get; set; } = PlanType.Free;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Store> Stores { get; set; } = [];
}
