using Inventra.Application.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace Inventra.Infrastructure.Security;

/// <summary>BCrypt implementation of IPasswordHasher. Work factor 12 is the production default.</summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string plainPassword) =>
        BC.HashPassword(plainPassword, WorkFactor);

    public bool Verify(string plainPassword, string hash) =>
        BC.Verify(plainPassword, hash);
}
