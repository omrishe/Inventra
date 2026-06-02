using Inventra.Application.DTOs.Auth;

namespace Inventra.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterCompanyAsync(RegisterCompanyRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(string clientToken, CancellationToken ct = default);
}
