/*using UnityEngine;

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
}*/
/*using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemType itemType = ItemType.Stone;
    [SerializeField] private int amount = 1;

    [Header("Usable Item")]
    [Tooltip("Wenn aktiv: heilt Player beim Aufsammeln um 20% und spielt Drink-Sound.")]
    [SerializeField] private bool usable = false;

    [Range(0f, 1f)]
    [SerializeField] private float healPercent = 0.2f; // 20%

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (InventoryManager.I == null)
            return;

        // ------------------------------------------------
        // ALWAYS add to inventory
        // ------------------------------------------------
        bool added = InventoryManager.I.TryAdd(itemType, amount);
        if (!added)
            return;

        if (debugLogs)
            Debug.Log($"Picked up {amount}x {itemType}");

        // ------------------------------------------------
        // OPTIONAL: usable effect
        // ------------------------------------------------
        if (usable)
        {
            PlayerHealth hp = other.GetComponent<PlayerHealth>();

            if (hp != null)
            {
                int healAmount =
                    Mathf.RoundToInt(hp.MaxHP * Mathf.Clamp01(healPercent));

                hp.Heal(healAmount);

                // Drink Sound
                FX_SoundSystem.I?.PlayHit("Drink", true);

                if (debugLogs)
                    Debug.Log($"Usable item healed player for {healAmount}");
            }
        }

        Destroy(gameObject);
    }
}*/
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemType itemType = ItemType.Stone;
    [SerializeField] private int amount = 1;

    [Header("Usable Item")]
    [Tooltip("Wenn aktiv: heilt Player beim Aufsammeln um 20% und spielt Drink-Sound.")]
    [SerializeField] private bool usable = false;

    [Range(0f, 1f)]
    [SerializeField] private float healPercent = 0.2f; // 20%

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    /// <summary>
    /// Wird z.B. von ThrowableFlight benutzt, um Drops nach dem Wurf "nicht-usable" zu machen.
    /// </summary>
    public void SetUsable(bool value)
    {
        usable = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (InventoryManager.I == null)
            return;

        // ALWAYS add to inventory
        bool added = InventoryManager.I.TryAdd(itemType, amount);
        if (!added)
            return;

        if (debugLogs)
            Debug.Log($"Picked up {amount}x {itemType}");

        // OPTIONAL: usable effect (only if usable==true)
        if (usable)
        {
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                int healAmount = Mathf.RoundToInt(hp.MaxHP * Mathf.Clamp01(healPercent));
                hp.Heal(healAmount);

                // Drink Sound (fix "Clap"/etc: tag is hardcoded by you)
                FX_SoundSystem.I?.PlayHit("Drink", true);

                if (debugLogs)
                    Debug.Log($"Usable item healed player for {healAmount}");
            }
        }

        Destroy(gameObject);
    }
}
