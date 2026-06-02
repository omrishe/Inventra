using Inventra.Domain.Enums;
using Inventra.Domain.Interfaces;

namespace Inventra.Domain.Entities;

/// <summary>
/// A user account belonging to a Chain tenant.
/// ChainAdmin users have no store scope (StoreId = null).
/// StoreManager/StoreEmployee are always scoped to a specific store.
/// </summary>
public class User : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid ChainId { get; set; }
    public Guid? StoreId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Chain Chain { get; set; } = null!;
    public Store? Store { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
