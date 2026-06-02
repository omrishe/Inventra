using Inventra.Domain.Enums;

namespace Inventra.Application.Constants;

/// <summary>
/// Maps each static role to its immutable permission set.
/// This is the single place to change role capabilities during the MVP phase.
/// </summary>
public static class RolePermissions
{
    private static readonly IReadOnlyDictionary<UserRole, IReadOnlyList<string>> _map =
        new Dictionary<UserRole, IReadOnlyList<string>>
        {
            [UserRole.ChainAdmin] =
            [
                Permissions.StoresRead,    Permissions.StoresWrite,
                Permissions.ProductsRead,  Permissions.ProductsWrite,
                Permissions.InventoryRead, Permissions.InventoryWrite, Permissions.InventoryAdjust
            ],
            [UserRole.StoreManager] =
            [
                Permissions.ProductsRead,
                Permissions.InventoryRead, Permissions.InventoryWrite, Permissions.InventoryAdjust
            ],
            [UserRole.StoreEmployee] =
            [
                Permissions.ProductsRead,
                Permissions.InventoryRead, Permissions.InventoryAdjust
            ]
        };

    public static IReadOnlyList<string> GetPermissions(UserRole role) =>
        _map.TryGetValue(role, out var perms) ? perms : [];
}
