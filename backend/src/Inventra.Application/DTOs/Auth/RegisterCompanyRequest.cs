namespace Inventra.Application.DTOs.Auth;

public sealed record RegisterCompanyRequest(
    string CompanyName,
    string Email,
    string Password
);
