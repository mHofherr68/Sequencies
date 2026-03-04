using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central inventory state and messaging hub.
/// 
/// Responsibilities:
/// - Stores item counts per ItemType and enforces per-item capacity (caps).
/// - Provides an active/selected item (used for throwing / UI selection).
/// - Raises events for UI updates (OnItemChanged, OnSelectedChanged) and messages (OnMessage).
/// - Emits German UI messages (consistent with InventoryUI).
/// 
/// This class is implemented as a simple scene singleton (InventoryManager.I).
/// </summary>
public class InventoryManager : MonoBehaviour
{
    /// <summary>Global singleton instance.</summary>
    public static InventoryManager I { get; private set; }

    [Header("Caps")]

    [Tooltip("Maximum number of stones the player can carry.")]
    [SerializeField] private int maxStones = 5;

    [Tooltip("Maximum number of books the player can carry.")]
    [SerializeField] private int maxBooks = 1;

    [Tooltip("Maximum number of crosses (Bottle slot) the player can carry.")]
    [SerializeField] private int maxBottles = 2;

    [Tooltip("Maximum number of drinks the player can carry.")]
    [SerializeField] private int maxDrinks = 2;

    /// <summary>
    /// Raised when an item count changes.
    /// Parameters: (itemType, newCount, cap)
    /// </summary>
    public event Action<ItemType, int, int> OnItemChanged;

    /// <summary>
    /// Raised when a UI message should be shown (German message strings).
    /// </summary>
    public event Action<string> OnMessage;

    /// <summary>
    /// Raised when the selected item changes.
    /// Parameter: (selectedItemType)
    /// </summary>
    public event Action<ItemType> OnSelectedChanged;

    /// <summary>Runtime item counts per ItemType.</summary>
    private readonly Dictionary<ItemType, int> counts = new();

    /// <summary>Capacity per ItemType.</summary>
    private readonly Dictionary<ItemType, int> caps = new();

    [SerializeField]
    [Tooltip("Currently selected inventory item (used by UI and throwing).")]
    private ItemType selectedItem = ItemType.Stone;

    /// <summary>Currently selected item type.</summary>
    public ItemType SelectedItem => selectedItem;

    private void Awake()
    {
        // Singleton guard
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        // Build caps table (clamped to >= 0)
        caps[ItemType.Stone] = Mathf.Max(0, maxStones);
        caps[ItemType.Book] = Mathf.Max(0, maxBooks);
        caps[ItemType.Bottle] = Mathf.Max(0, maxBottles);
        caps[ItemType.Drink] = Mathf.Max(0, maxDrinks);

        // Initialize counts for all ItemTypes
        foreach (ItemType t in Enum.GetValues(typeof(ItemType)))
            counts[t] = 0;
    }

    /// <summary>
    /// Returns current count of the given item type.
    /// </summary>
    public int GetCount(ItemType type) => counts.TryGetValue(type, out var c) ? c : 0;

    /// <summary>
    /// Returns the capacity (cap) of the given item type.
    /// </summary>
    public int GetCap(ItemType type) => caps.TryGetValue(type, out var c) ? c : 0;

    /// <summary>
    /// Sets the selected item and notifies listeners.
    /// </summary>
    public void Select(ItemType type)
    {
        selectedItem = type;
        OnSelectedChanged?.Invoke(type);
    }

    /// <summary>
    /// Tries to add items to the inventory, respecting caps.
    /// Shows a German message if the cap is reached.
    /// </summary>
    /// <param name="type">Item type to add.</param>
    /// <param name="amount">Amount to add (must be > 0).</param>
    /// <returns>True if at least one item was added.</returns>
    public bool TryAdd(ItemType type, int amount = 1)
    {
        if (amount <= 0) return false;

        int cap = GetCap(type);
        int cur = GetCount(type);

        // Already at cap
        if (cur >= cap)
        {
            OnMessage?.Invoke($"Mehr {GetGermanItemName(type)} nicht möglich!");
            return false;
        }

        // Add up to remaining space
        int add = Mathf.Min(amount, cap - cur);
        counts[type] = cur + add;

        OnItemChanged?.Invoke(type, counts[type], cap);

        // Partial add -> message
        if (add < amount)
            OnMessage?.Invoke($"Mehr {GetGermanItemName(type)} nicht möglich!");

        return add > 0;
    }

    /// <summary>
    /// Tries to consume (remove) items from the inventory.
    /// Shows a German message if not enough items are available.
    /// </summary>
    /// <param name="type">Item type to consume.</param>
    /// <param name="amount">Amount to consume (must be > 0).</param>
    /// <returns>True if items were consumed successfully.</returns>
    public bool TryConsume(ItemType type, int amount = 1)
    {
        if (amount <= 0) return false;

        int cur = GetCount(type);
        if (cur < amount)
        {
            OnMessage?.Invoke($"Keine {GetGermanItemName(type)} im Inventar!");
            return false;
        }

        counts[type] = cur - amount;
        OnItemChanged?.Invoke(type, counts[type], GetCap(type));
        return true;
    }

    /// <summary>
    /// Sends a UI message to listeners.
    /// </summary>
    public void ShowMessage(string msg)
    {
        if (string.IsNullOrWhiteSpace(msg)) return;
        OnMessage?.Invoke(msg);
    }

    /// <summary>
    /// German display names (kept consistent with InventoryUI).
    /// </summary>
    private static string GetGermanItemName(ItemType type)
    {
        switch (type)
        {
            case ItemType.Stone: return "Steine";
            case ItemType.Book: return "Bücher";
            case ItemType.Bottle: return "Kreuze";
            case ItemType.Drink: return "Getränke";
            default: return type.ToString();
        }
    }
}