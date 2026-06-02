using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Inventra.Application.Constants;
using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Inventra.Infrastructure.Security;

/// <summary>
/// Handles JWT access token generation and refresh token lifecycle using the
/// ASP.NET Core Data Protection API for tamper-proof opaque token creation.
/// </summary>
public sealed class TokenService : ITokenService
{
    private const string DataProtectionPurpose = "Inventra.RefreshToken.v1";

    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;
    private readonly IDataProtector _protector;

    public TokenService(IConfiguration configuration, IDataProtectionProvider dataProtectionProvider)
    {
        _key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is not configured.");
        _issuer = configuration["Jwt:Issuer"]!;
        _audience = configuration["Jwt:Audience"]!;
        _expiryMinutes = configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
        _protector = dataProtectionProvider.CreateProtector(DataProtectionPurpose);
    }

    // ── Access Token ──────────────────────────────────────────────────────────
    public string GenerateAccessToken(User user, IReadOnlyList<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new("chainId", user.ChainId.ToString()),
            new("role",    user.Role.ToString()),
        };

        if (user.StoreId.HasValue)
            claims.Add(new Claim("storeId", user.StoreId.Value.ToString()));

        // Each permission is its own claim — avoids parsing a delimited string.
        foreach (var perm in permissions)
            claims.Add(new Claim(Permissions.ClaimType, perm));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ── Refresh Token ─────────────────────────────────────────────────────────
    public RefreshTokenResult GenerateRefreshToken(Guid userId)
    {
        // Encrypt a payload containing userId + random nonce using Data Protection API.
        var payload = JsonSerializer.Serialize(new { UserId = userId, Nonce = Guid.NewGuid() });
        var clientToken = _protector.Protect(payload);

        // Only the SHA-256 hash is stored in the DB — the raw encrypted token is never persisted.
        var tokenHash = HashToken(clientToken);

        return new RefreshTokenResult(clientToken, tokenHash);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool TryUnprotect(string clientToken, out Guid userId)
    {
        userId = Guid.Empty;
        try
        {
            var json = _protector.Unprotect(clientToken);
            var doc = JsonDocument.Parse(json);
            userId = doc.RootElement.GetProperty("UserId").GetGuid();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
