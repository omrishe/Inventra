using Inventra.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventra.Application.Interfaces;

/// <summary>
/// Application-layer abstraction over the EF Core DbContext.
/// Keeps the Application project free of any Infrastructure dependency.
/// </summary>
public interface IAppDbContext
{
    DbSet<Chain> Chains { get; }
    DbSet<Store> Stores { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
