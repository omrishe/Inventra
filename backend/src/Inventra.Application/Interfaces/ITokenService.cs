using Inventra.Domain.Entities;

namespace Inventra.Application.Interfaces;

public sealed record RefreshTokenResult(string ClientToken, string TokenHash);

public interface ITokenService
{
    /// <summary>Generates a signed 15-minute JWT embedding claims and permissions.</summary>
    string GenerateAccessToken(User user, IReadOnlyList<string> permissions);

    /// <summary>
    /// Creates a new opaque refresh token.
    /// ClientToken  = Data Protection API protected payload (sent to client).
    /// TokenHash    = SHA-256 of ClientToken (stored in DB for lookup).
    /// </summary>
    RefreshTokenResult GenerateRefreshToken(Guid userId);

    /// <summary>Hashes a token string with SHA-256 for DB lookup.</summary>
    string HashToken(string token);

    /// <summary>Attempts to unprotect the opaque client token and extract the UserId.</summary>
    bool TryUnprotect(string clientToken, out Guid userId);
}
