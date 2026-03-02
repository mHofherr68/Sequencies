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

    [Header("Ghost")]
    [Tooltip("Optional: im Inspector setzen. Wenn leer, nutzt er GhostSpawner.I (persistent).")]
    [SerializeField] private GhostSpawner ghostSpawner;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private PlayerController playerController;

    private void Awake()
    {
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

        // Only USABLE items trigger Drink sound + (optional) ghost spawn
        if (usable)
        {
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                int healAmount = Mathf.RoundToInt(hp.MaxHP * Mathf.Clamp01(healPercent));
                hp.Heal(healAmount);

                // Drink Sound (always when usable triggers)
                FX_SoundSystem.I?.PlayHit("Drink", true);

                if (debugLogs)
                    Debug.Log($"Usable item healed player for {healAmount}", this);
            }

            // Ghost spawn ONLY for usable items AND only if PlayerController allows it
            if (playerController == null)
                playerController = other.GetComponentInParent<PlayerController>();

            if (playerController != null && playerController.GhostSpawningEnabled)
            {
                GhostSpawner spawner = ghostSpawner != null ? ghostSpawner : GhostSpawner.I;
                spawner?.SpawnGhostTo(transform.position);
            }
        }

        Destroy(gameObject);
    }
}
