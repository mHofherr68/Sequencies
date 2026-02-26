/*using UnityEngine;            / Working fine!

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemType itemType = ItemType.Stone;
    [SerializeField] private int amount = 1;

    [Header("Usable Item")]
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
}*/
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemType itemType = ItemType.Stone;
    [SerializeField] private int amount = 1;

    [Header("Usable Item")]
    [SerializeField] private bool usable = false;

    [Range(0f, 1f)]
    [SerializeField] private float healPercent = 0.2f; // 20%

    // =====================================================
    // Ghost (spawn on pickup) - same pattern as PlayerController
    // =====================================================
    [Header("Ghost (optional spawn on pickup)")]
    [Tooltip("Wenn true: dieses Pickup löst einen Ghost aus (spawn/retarget) beim Aufheben.")]
    [SerializeField] private bool enableSpawn = false;

    [Tooltip("Optional: im Inspector setzen. Wenn leer, nutzt er GhostSpawner.I (persistent).")]
    [SerializeField] private GhostSpawner ghostSpawner;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private void Awake()
    {
        // Auto-resolve persistent spawner if inspector ref is missing (Prefab / Level 2/3 case)
        if (ghostSpawner == null)
            ghostSpawner = GhostSpawner.I;
    }

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
            Debug.Log($"Picked up {amount}x {itemType}", this);

        // OPTIONAL: usable effect (ONLY if usable==true)  -> UNCHANGED
        if (usable)
        {
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                int healAmount = Mathf.RoundToInt(hp.MaxHP * Mathf.Clamp01(healPercent));
                hp.Heal(healAmount);

                // Drink Sound (UNCHANGED)
                FX_SoundSystem.I?.PlayHit("Drink", true);

                if (debugLogs)
                    Debug.Log($"Usable item healed player for {healAmount}", this);
            }
        }

        // OPTIONAL: spawn/retarget ghost (independent of usable) -> PlayerController pattern
        if (enableSpawn)
        {
            GhostSpawner spawner = ghostSpawner != null ? ghostSpawner : GhostSpawner.I;
            if (spawner != null)
            {
                spawner.SpawnGhostTo(transform.position);

                if (debugLogs)
                    Debug.Log("[ItemPickup] Ghost spawn/retarget triggered.", this);
            }
            else
            {
                if (debugLogs)
                    Debug.LogWarning("[ItemPickup] enableSpawn=true but no GhostSpawner found (ghostSpawner and GhostSpawner.I are null).", this);
            }
        }

        Destroy(gameObject);
    }
}