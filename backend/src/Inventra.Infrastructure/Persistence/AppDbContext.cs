using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Inventra.Infrastructure.Persistence;

/// <summary>
/// The application's single EF Core DbContext.
/// Implements IAppDbContext so the Application layer never references Infrastructure directly.
///
/// Key behaviours:
/// 1. Global Query Filters — every ITenantEntity is auto-scoped to the active ChainId.
/// 2. xmin concurrency token — wired on InventoryItem (Phase 4).
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
    : DbContext(options), IAppDbContext
{
    // ── DbSets ─────────────────────────────────────────────────────────────────
    public DbSet<Chain> Chains => Set<Chain>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // ── IAppDbContext: transaction helper ──────────────────────────────────────
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => await Database.BeginTransactionAsync(ct);

    // ── Model Configuration ────────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Global Query Filter: auto-applied to every ITenantEntity ──────────
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
            var property  = System.Linq.Expressions.Expression.Property(parameter, nameof(ITenantEntity.ChainId));
            var tenantId  = System.Linq.Expressions.Expression.Constant(tenantContext.ChainId);
            var equals    = System.Linq.Expressions.Expression.Equal(property, tenantId);
            var lambda    = System.Linq.Expressions.Expression.Lambda(equals, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }

        // ── Chain ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Chain>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.PlanType).HasConversion<string>();
        });

        // ── Store ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Store>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.Property(s => s.Location).HasMaxLength(500);
            e.HasOne(s => s.Chain).WithMany(c => c.Stores)
             .HasForeignKey(s => s.ChainId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── User ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(320);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).HasConversion<string>();
            e.HasOne(u => u.Chain).WithMany()
             .HasForeignKey(u => u.ChainId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(u => u.Store).WithMany()
             .HasForeignKey(u => u.StoreId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
        });

        // ── RefreshToken ──────────────────────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(rt => rt.Id);
            e.HasIndex(rt => rt.TokenHash).IsUnique();
            e.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(64);
            e.HasOne(rt => rt.User).WithMany(u => u.RefreshTokens)
             .HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
