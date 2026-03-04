/// <summary>
/// Defines all item types used in the inventory system.
/// 
/// These values are used for:
/// - Inventory storage (InventoryManager)
/// - UI slots (InventoryUI)
/// - Projectile mapping (PlayerThrower)
/// - Item pickups (ItemPickup)
/// </summary>
public enum ItemType
{
    /// <summary>Throwable stone projectile.</summary>
    Stone,

    /// <summary>Throwable book projectile.</summary>
    Book,

    /// <summary>Cross item used against the ghost (stored in the bottle slot).</summary>
    Bottle,

    /// <summary>Usable healing drink item.</summary>
    Drink
}