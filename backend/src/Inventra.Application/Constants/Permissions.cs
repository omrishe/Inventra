namespace Inventra.Application.Constants;

/// <summary>
/// All permission strings used in the system.
/// These are embedded as JWT claims and checked by [HasPermission].
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permission";

    public const string StoresRead    = "stores:read";
    public const string StoresWrite   = "stores:write";

    public const string ProductsRead  = "products:read";
    public const string ProductsWrite = "products:write";

    public const string InventoryRead    = "inventory:read";
    public const string InventoryWrite   = "inventory:write";
    public const string InventoryAdjust  = "inventory:adjust";
}
