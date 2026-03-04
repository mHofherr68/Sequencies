using UnityEngine;

/// <summary>
/// Adds an item to the player's inventory when the player enters the trigger.
/// 
/// Responsibilities:
/// - Detects player trigger entry and adds the configured ItemType/amount to InventoryManager.
/// - Supports "usable" pickups (e.g. Drink):
///   - Heals the player by a percentage of max HP.
///   - Plays the "Drink" sound.
///   - Optionally spawns a non-static noise light prefab at the pickup position.
///   - Optionally triggers a ghost retarget/spawn, but only if the PlayerController allows it.
/// - Destroys the pickup object after a successful pickup.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Tooltip("The item type that will be added to the inventory when picked up.")]
    [SerializeField] private ItemType itemType = ItemType.Stone;

    [Tooltip("How many items are added on pickup.")]
    [SerializeField] private int amount = 1;

    [Header("Usable Item")]

    [Tooltip("If true, this pickup applies an immediate effect (heal + sound), e.g. Drink.")]
    [SerializeField] private bool usable = false;

    [Tooltip("Heal percentage of the player's max HP (0..1). Example: 0.2 = 20%.")]
    [Range(0f, 1f)]
    [SerializeField] private float healPercent = 0.2f; // 20%

    [Header("Ghost")]

    [Tooltip("Optional: assign in Inspector. If empty, uses GhostSpawner.I (persistent singleton).")]
    [SerializeField] private GhostSpawner ghostSpawner;

    [Header("Noise Light (non-static)")]

    [Tooltip("Prefab with a Point Light 2D + NoiseLightLifetime (fade & destroy). Spawned at the pickup position when a usable (Drink) pickup triggers.")]
    [SerializeField] private GameObject noiseLightPrefab;

    [Header("Debug")]

    [Tooltip("If true, logs pickup/heal information to the console.")]
    [SerializeField] private bool debugLogs = false;

    // Cached while interacting (used to check GhostSpawningEnabled flag).
    private PlayerController playerController;

    private void Awake()
    {
        // Auto-resolve persistent spawner if inspector reference is missing.
        if (ghostSpawner == null)
            ghostSpawner = GhostSpawner.I;
    }

    private void Reset()
    {
        // Enforce trigger behavior.
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    /// <summary>
    /// Used by systems like ThrowableFlight to disable "usable" behavior for dropped/spawned items.
    /// </summary>
    public void SetUsable(bool value)
    {
        usable = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the player can pick this up.
        if (!other.CompareTag("Player"))
            return;

        if (InventoryManager.I == null)
            return;

        // Always add to inventory (if possible).
        bool added = InventoryManager.I.TryAdd(itemType, amount);
        if (!added)
            return;

        if (debugLogs)
            Debug.Log($"Picked up {amount}x {itemType}", this);

        // Only usable pickups trigger heal + drink sound + optional effects.
        if (usable)
        {
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                int healAmount = Mathf.RoundToInt(hp.MaxHP * Mathf.Clamp01(healPercent));
                hp.Heal(healAmount);

                // Drink sound (always when usable triggers).
                FX_SoundSystem.I?.PlayHit("Drink", true);

                // Non-static noise light at pickup position.
                if (noiseLightPrefab != null)
                {
                    Vector3 p = transform.position;
                    p.z = 0f;
                    Instantiate(noiseLightPrefab, p, Quaternion.identity);
                }

                if (debugLogs)
                    Debug.Log($"Usable item healed player for {healAmount}", this);
            }

            // Ghost spawn/retarget ONLY for usable pickups AND only if PlayerController allows it.
            if (playerController == null)
                playerController = other.GetComponentInParent<PlayerController>();

            if (playerController != null && playerController.GhostSpawningEnabled)
            {
                GhostSpawner spawner = ghostSpawner != null ? ghostSpawner : GhostSpawner.I;
                spawner?.SpawnGhostTo(transform.position);
            }
        }

        // Remove pickup object after successful pickup.
        Destroy(gameObject);
    }
}