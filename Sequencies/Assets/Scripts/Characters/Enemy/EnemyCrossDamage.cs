/*using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyCrossDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private bool damageToEnemy = true;

    [Tooltip("Schaden für den Ghost (z.B. 10 = 10% bei 100HP).")]
    [SerializeField] private int damageAmount = 10;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private bool hasDamaged = false;

    private void Reset()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c != null)
            c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!damageToEnemy) return;
        if (hasDamaged) return;

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null) return;

        enemy.Damage(damageAmount);

        hasDamaged = true;

        if (debugLog)
            Debug.Log($"[EnemyCrossDamage] Ghost damaged for {damageAmount}.", this);
    }
}*/
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyCrossDamage : MonoBehaviour
{
    [Header("Damage")]
    [Tooltip("Startschaden (z.B. 10 = 10% bei 100HP).")]
    [SerializeField] private int startDamage = 10;

    [Tooltip("Schaden wird pro Nutzung reduziert.")]
    [SerializeField] private int damageDecay = 2;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    [SerializeField] private int currentDamage;

    // merkt sich Damage auch wenn Kreuz aufgehoben und neu gespawnt wird
    private static int savedDamage = -1;

    private void Awake()
    {
        if (savedDamage < 0)
            savedDamage = startDamage;

        currentDamage = savedDamage;
    }

    private void Reset()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c != null)
            c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentDamage <= 0) return;

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null) return;

        enemy.Damage(currentDamage);

        if (debugLog)
            Debug.Log($"[Cross] Ghost hit for {currentDamage}", this);

        currentDamage -= damageDecay;

        // Damage speichern (wichtig für Throw ? Respawn)
        savedDamage = currentDamage;

        if (currentDamage <= 0)
        {
            if (debugLog)
                Debug.Log("[Cross] Cross consumed", this);

            Destroy(gameObject);
        }
    }
}