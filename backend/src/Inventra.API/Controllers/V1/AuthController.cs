using Inventra.Application.DTOs.Auth;
using Inventra.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventra.API.Controllers.V1;

/// <summary>
/// Handles company registration, login, and token refresh.
/// All endpoints are intentionally anonymous — no [Authorize] required.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Registers a new company (Chain) and creates the initial ChainAdmin account.
    /// Returns a JWT access token and opaque refresh token on success.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCompanyRequest request, CancellationToken ct)
    {
        try
        {
            var response = await authService.RegisterCompanyAsync(request, ct);
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Authenticates a user and returns a fresh JWT + refresh token pair.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var response = await authService.LoginAsync(request, ct);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            // Return a generic message to prevent user enumeration.
            return Unauthorized(new { error = "Invalid email or password." });
        }
    }

    /// <summary>
    /// Exchanges a valid refresh token for a new JWT + rotated refresh token.
    /// The old refresh token is revoked upon successful rotation.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        try
        {
            var response = await authService.RefreshTokenAsync(request.RefreshToken, ct);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}

// ── Inline request record (too small to warrant its own file) ─────────────────
public sealed record RefreshTokenRequest(string RefreshToken);
