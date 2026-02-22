using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemType itemType = ItemType.Stone;
    [SerializeField] private int amount = 1;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (debugLogs) Debug.Log($"Pickup Trigger Enter: {name} hit {other.name}", this);

        if (!other.CompareTag("Player")) return;

        if (InventoryManager.I == null)
        {
            if (debugLogs) Debug.LogWarning("No InventoryManager in scene.", this);
            return;
        }

        bool added = InventoryManager.I.TryAdd(itemType, amount);
        if (added)
        {
            if (debugLogs) Debug.Log($"Picked up {amount}x {itemType}", this);
            Destroy(gameObject);
        }
        else
        {
            if (debugLogs) Debug.Log($"Inventory full for {itemType}", this);
        }
    }
}