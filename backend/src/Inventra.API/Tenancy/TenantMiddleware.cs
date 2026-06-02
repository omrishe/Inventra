using Inventra.Application.Interfaces;
using System.Security.Claims;

namespace Inventra.API.Tenancy;

/// <summary>
/// Middleware that runs after authentication (UseAuthentication) and extracts
/// tenant identifiers from the validated JWT, populating the scoped ITenantContext.
///
/// Bypass paths (Login / Register) are skipped so anonymous access still works.
/// </summary>
public class TenantMiddleware(RequestDelegate next)
{
    // These paths do not require tenant context because the user is not yet authenticated.
    private static readonly HashSet<string> _bypassPaths =
    [
        "/api/v1/auth/login",
        "/api/v1/auth/register",
        "/api/v1/auth/refresh"
    ];

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // Skip tenant extraction for unauthenticated endpoints.
        if (_bypassPaths.Contains(path))
        {
            await next(context);
            return;
        }

        // If the user is not authenticated, let the authorization middleware return 401.
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        // ── Extract ChainId (required) ─────────────────────────────────────────
        var chainIdClaim = context.User.FindFirstValue("chainId");
        if (string.IsNullOrWhiteSpace(chainIdClaim) || !Guid.TryParse(chainIdClaim, out var chainId))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Missing or invalid 'chainId' claim.");
            return;
        }

        tenantContext.ChainId = chainId;

        // ── Extract StoreId (optional — ChainAdmin users have no store scope) ──
        var storeIdClaim = context.User.FindFirstValue("storeId");
        if (!string.IsNullOrWhiteSpace(storeIdClaim) && Guid.TryParse(storeIdClaim, out var storeId))
        {
            tenantContext.StoreId = storeId;
        }

        await next(context);
    }
}
