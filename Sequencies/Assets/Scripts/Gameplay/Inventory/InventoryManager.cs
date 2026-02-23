using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager I { get; private set; }

    [Header("Caps")]
    [SerializeField] private int maxStones = 5;
    [SerializeField] private int maxBooks = 1;
    [SerializeField] private int maxBottles = 2;
    [SerializeField] private int maxDrinks = 2;

    public event Action<ItemType, int, int> OnItemChanged; // type, count, cap
    public event Action<string> OnMessage;                 // UI message
    public event Action<ItemType> OnSelectedChanged;

    private readonly Dictionary<ItemType, int> counts = new();
    private readonly Dictionary<ItemType, int> caps = new();

    [SerializeField] private ItemType selectedItem = ItemType.Stone;
    public ItemType SelectedItem => selectedItem;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        caps[ItemType.Stone] = Mathf.Max(0, maxStones);
        caps[ItemType.Book] = Mathf.Max(0, maxBooks);
        caps[ItemType.Bottle] = Mathf.Max(0, maxBottles);
        caps[ItemType.Drink] = Mathf.Max(0, maxDrinks);

        foreach (ItemType t in Enum.GetValues(typeof(ItemType)))
            counts[t] = 0;
    }

    public int GetCount(ItemType type) => counts.TryGetValue(type, out var c) ? c : 0;
    public int GetCap(ItemType type) => caps.TryGetValue(type, out var c) ? c : 0;

    public void Select(ItemType type)
    {
        selectedItem = type;
        OnSelectedChanged?.Invoke(type);
    }

    public bool TryAdd(ItemType type, int amount = 1)
    {
        if (amount <= 0) return false;

        int cap = GetCap(type);
        int cur = GetCount(type);

        if (cur >= cap)
        {
            OnMessage?.Invoke($"Max. {type} erreicht!");
            return false;
        }

        int add = Mathf.Min(amount, cap - cur);
        counts[type] = cur + add;

        OnItemChanged?.Invoke(type, counts[type], cap);

        if (add < amount)
            OnMessage?.Invoke($"Max. {type} erreicht!");

        return add > 0;
    }

    public bool TryConsume(ItemType type, int amount = 1)
    {
        if (amount <= 0) return false;

        int cur = GetCount(type);
        if (cur < amount)
        {
            OnMessage?.Invoke($"Kein {type} im Inventar!");
            return false;
        }

        counts[type] = cur - amount;
        OnItemChanged?.Invoke(type, counts[type], GetCap(type));
        return true;
    }

    public void ShowMessage(string msg)
    {
        if (string.IsNullOrWhiteSpace(msg)) return;
        OnMessage?.Invoke(msg);
    }
}