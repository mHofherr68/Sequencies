using UnityEngine;

/// <summary>
/// Handles damage applied to enemies when they collide with a Cross item.
/// 
/// The Cross acts as a reusable damage source that weakens over time.
/// Each successful hit reduces its damage value until it eventually
/// reaches zero and destroys itself.
/// 
/// The current damage value is stored statically so the Cross keeps its
/// remaining strength even if the object is picked up, thrown, or respawned.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyCrossDamage : MonoBehaviour
{
    [Header("Damage")]

    [Tooltip("Initial damage dealt to an enemy on contact (e.g. 10 = 10% when enemy has 100 HP).")]
    [SerializeField] private int startDamage = 10;

    [Tooltip("Amount of damage reduction applied after each successful hit.")]
    [SerializeField] private int damageDecay = 2;

    [Header("Debug")]

    [Tooltip("Enables debug logging for cross damage events.")]
    [SerializeField] private bool debugLog = false;

    /// <summary>
    /// Current damage value of the Cross.
    /// This decreases after each enemy hit.
    /// </summary>
    [SerializeField] private int currentDamage;

    /// <summary>
    /// Static storage for the Cross damage value.
    /// 
    /// This allows the Cross to preserve its remaining damage even if
    /// it is picked up, thrown, despawned, or respawned later.
    /// </summary>
    private static int savedDamage = -1;

    /// <summary>
    /// Initializes the damage value when the Cross instance is created.
    /// If a saved value exists, it will be restored.
    /// </summary>
    private void Awake()
    {
        if (savedDamage < 0)
            savedDamage = startDamage;

        currentDamage = savedDamage;
    }

    /// <summary>
    /// Ensures the collider is configured as a trigger when the component
    /// is first added in the editor.
    /// </summary>
    private void Reset()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c != null)
            c.isTrigger = true;
    }

    /// <summary>
    /// Called when another collider enters the Cross trigger.
    /// 
    /// If the collider belongs to an enemy with an EnemyHealth component,
    /// damage will be applied once and the Cross damage value will decrease.
    /// </summary>
    /// <param name="other">Collider that entered the trigger area.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentDamage <= 0) return;

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null) return;

        enemy.Damage(currentDamage);

        if (debugLog)
            Debug.Log($"[Cross] Ghost hit for {currentDamage}", this);

        currentDamage -= damageDecay;

        // Save the updated damage value for future Cross instances
        savedDamage = currentDamage;

        if (currentDamage <= 0)
        {
            if (debugLog)
                Debug.Log("[Cross] Cross consumed", this);

            Destroy(gameObject);
        }
    }
}