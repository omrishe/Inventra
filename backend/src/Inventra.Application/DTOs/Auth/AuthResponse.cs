namespace Inventra.Application.DTOs.Auth;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string Email,
    string Role,
    IReadOnlyList<string> Permissions
);
