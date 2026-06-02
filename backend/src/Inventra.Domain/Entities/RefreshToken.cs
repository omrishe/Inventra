namespace Inventra.Domain.Entities;

/// <summary>
/// Represents a hashed, revokable refresh token stored per user.
/// The actual opaque token string (protected by Data Protection API) never touches the DB —
/// only its SHA-256 hash is persisted for lookup.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty; // SHA-256 of the opaque client token
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}
