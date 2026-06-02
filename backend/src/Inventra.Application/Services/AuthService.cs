using Inventra.Application.Constants;
using Inventra.Application.DTOs.Auth;
using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Inventra.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Inventra.Application.Services;

public sealed class AuthService(
    IAppDbContext context,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IConfiguration configuration) : IAuthService
{
    private readonly int _refreshTokenExpiryDays =
        configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

    // ── Register ──────────────────────────────────────────────────────────────
    public async Task<AuthResponse> RegisterCompanyAsync(
        RegisterCompanyRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email, ct);

        if (emailExists)
            throw new InvalidOperationException("A user with this email already exists.");

        await using var tx = await context.BeginTransactionAsync(ct);
        try
        {
            var chain = new Chain { Id = Guid.NewGuid(), Name = request.CompanyName.Trim() };
            context.Chains.Add(chain);

            var admin = new User
            {
                Id = Guid.NewGuid(),
                ChainId = chain.Id,
                StoreId = null,
                Email = email,
                PasswordHash = passwordHasher.Hash(request.Password),
                Role = UserRole.ChainAdmin
            };
            context.Users.Add(admin);
            await context.SaveChangesAsync(ct);

            var authResponse = await IssueTokensAsync(admin, ct);
            await tx.CommitAsync(ct);
            return authResponse;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // ── Login ─────────────────────────────────────────────────────────────────
    public async Task<AuthResponse> LoginAsync(
        LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await IssueTokensAsync(user, ct);
    }

    // ── Refresh ───────────────────────────────────────────────────────────────
    public async Task<AuthResponse> RefreshTokenAsync(
        string clientToken, CancellationToken ct = default)
    {
        var hash = tokenService.HashToken(clientToken);

        var stored = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token is invalid or has expired.");

        if (!tokenService.TryUnprotect(clientToken, out var userId) || userId != stored.UserId)
            throw new UnauthorizedAccessException("Refresh token integrity check failed.");

        stored.IsRevoked = true;
        await context.SaveChangesAsync(ct);

        return await IssueTokensAsync(stored.User, ct);
    }

    // ── Shared helper ─────────────────────────────────────────────────────────
    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
        var permissions = RolePermissions.GetPermissions(user.Role);
        var accessToken = tokenService.GenerateAccessToken(user, permissions);
        var (clientToken, tokenHash) = tokenService.GenerateRefreshToken(user.Id);

        context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            IsRevoked = false
        });
        await context.SaveChangesAsync(ct);

        return new AuthResponse(accessToken, clientToken, user.Email, user.Role.ToString(), permissions);
    }
}
