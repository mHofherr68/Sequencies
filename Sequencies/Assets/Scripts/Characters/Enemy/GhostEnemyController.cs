/*using UnityEngine;

public class GhostEnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Tooltip("Wie nah muss der Ghost am Ziel sein, um es als erreicht zu zählen?")]
    [SerializeField] private float arriveDistance = 0.15f;

    [Header("Lifetime")]
    [Tooltip("Wenn > 0: Ghost despawnt automatisch nach X Sekunden (Safety). 0 = aus.")]
    [SerializeField] private float maxLifetime = 0f;

    [Header("Player Contact (optional)")]
    [Tooltip("Tag des Player-Objekts. Muss beim Player gesetzt sein.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Cooldown, damit Kontakt nicht jede Frame spammt (später für Damage).")]
    [SerializeField] private float contactCooldown = 0.6f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;
    [SerializeField] private bool drawDebugTarget = true;

    private Vector2 targetPos;
    private bool hasTarget;

    private float spawnTime;
    private float lastContactTime = -999f;

    private void OnEnable()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        // Safety despawn (optional)
        if (maxLifetime > 0f && Time.time >= spawnTime + maxLifetime)
        {
            if (debugLog) Debug.Log("[GhostEnemy] Despawn (maxLifetime).");
            Destroy(gameObject);
            return;
        }

        // No target => idle (stable)
        if (!hasTarget)
            return;

        Vector2 pos = transform.position;
        Vector2 next = Vector2.MoveTowards(pos, targetPos, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(next.x, next.y, transform.position.z);

        // Arrived?
        if (Vector2.Distance(next, targetPos) <= arriveDistance)
        {
            hasTarget = false;

            if (debugLog) Debug.Log("[GhostEnemy] Arrived at target.");

            // Für später: jetzt könntest du z.B. "idle", "search", "run away" triggern.
            // Für den MVP lassen wir ihn einfach stehen.
        }
    }

    /// <summary>
    /// Setzt das Ziel, zu dem der Ghost laufen soll.
    /// Kann jederzeit aufgerufen werden (z.B. bei Geräusch).
    /// </summary>
    public void SetTarget(Vector2 worldPos)
    {
        targetPos = worldPos;
        hasTarget = true;

        if (debugLog) Debug.Log($"[GhostEnemy] Target set: {targetPos}");
    }

    /// <summary>
    /// Optional: Ghost stoppt und hat kein Ziel mehr.
    /// </summary>
    public void ClearTarget()
    {
        hasTarget = false;
    }

    // Player contact: bewusst super simpel gehalten.
    // Wir ändern KEIN Health-System hier. Das kommt später.
    private void OnTriggerStay2D(Collider2D other)
    {
        if (string.IsNullOrEmpty(playerTag)) return;
        if (!other.CompareTag(playerTag)) return;

        if (Time.time < lastContactTime + contactCooldown)
            return;

        lastContactTime = Time.time;

        // Platzhalter für später:
        // other.GetComponent<PlayerHealth>()?.TakeDamage(10);
        if (debugLog) Debug.Log("[GhostEnemy] Contact with Player (placeholder).");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawDebugTarget) return;
        if (!hasTarget) return;

        Gizmos.DrawWireSphere(targetPos, 0.15f);
        Gizmos.DrawLine(transform.position, (Vector3)targetPos);
    }
#endif
}*/
/*using UnityEngine;

public class GhostEnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float arriveDistance = 0.15f;

    [Header("Lifetime")]
    [Tooltip("Ghost despawnt nach X Sekunden automatisch.")]
    [SerializeField] private float despawnAfterSeconds = 3f;

    [Header("Player Damage")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Schaden pro Tick. Für 10%-HUD: 10 = 10%.")]
    [SerializeField] private int damagePerTick = 10;

    [Tooltip("Zeit zwischen Damage-Ticks, solange der Ghost am Player dran ist.")]
    [SerializeField] private float damageInterval = 0.75f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;
    [SerializeField] private bool drawDebugTarget = true;

    private Vector2 targetPos;
    private bool hasTarget;

    private float spawnTime;

    // damage tick state
    private float nextDamageTime = 0f;

    private void OnEnable()
    {
        spawnTime = Time.time;
        nextDamageTime = 0f;
    }

    private void Update()
    {
        // Despawn timer (stabil, unabhängig von allem)
        if (despawnAfterSeconds > 0f && Time.time >= spawnTime + despawnAfterSeconds)
        {
            if (debugLog) Debug.Log("[GhostEnemy] Despawn (timer).");
            Destroy(gameObject);
            return;
        }

        // Idle wenn kein Ziel
        if (!hasTarget)
            return;

        Vector2 pos = transform.position;
        Vector2 next = Vector2.MoveTowards(pos, targetPos, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(next.x, next.y, transform.position.z);

        if (Vector2.Distance(next, targetPos) <= arriveDistance)
        {
            hasTarget = false;
            if (debugLog) Debug.Log("[GhostEnemy] Arrived at target.");
        }
    }

    public void SetTarget(Vector2 worldPos)
    {
        targetPos = worldPos;
        hasTarget = true;

        if (debugLog) Debug.Log($"[GhostEnemy] Target set: {targetPos}");
    }

    public void ClearTarget()
    {
        hasTarget = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (string.IsNullOrEmpty(playerTag)) return;
        if (!other.CompareTag(playerTag)) return;

        // Tick-Schaden (kein Frame-Spam)
        if (Time.time < nextDamageTime) return;

        nextDamageTime = Time.time + Mathf.Max(0.05f, damageInterval);

        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.Damage(damagePerTick);
            if (debugLog) Debug.Log($"[GhostEnemy] Damage {damagePerTick} to Player.");
        }
        else
        {
            if (debugLog) Debug.Log("[GhostEnemy] Player has no PlayerHealth component.");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawDebugTarget) return;
        if (!hasTarget) return;

        Gizmos.DrawWireSphere(targetPos, 0.15f);
        Gizmos.DrawLine(transform.position, (Vector3)targetPos);
    }
#endif
}*/
using UnityEngine;

public class GhostEnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float arriveDistance = 0.15f;

    [Header("Lifetime")]
    [Tooltip("Ghost despawnt nach X Sekunden automatisch.")]
    [SerializeField] private float despawnAfterSeconds = 3f;

    [Header("Player Damage")]
    [Tooltip("Schaden pro Tick. Für 10%-HUD: 10 = 10%.")]
    [SerializeField] private int damagePerTick = 10;

    [Tooltip("Zeit zwischen Damage-Ticks, solange der Ghost am Player dran ist.")]
    [SerializeField] private float damageInterval = 0.75f;

    [Header("Optional Tag Filter (Root Tag)")]
    [Tooltip("Wenn leer: kein Tag-Check. Wenn gesetzt: Root muss diesen Tag haben (z.B. 'Player').")]
    [SerializeField] private string requiredRootTag = "Player";

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;
    [SerializeField] private bool drawDebugTarget = true;

    private Vector2 targetPos;
    private bool hasTarget;

    private float spawnTime;

    // damage tick state
    private float nextDamageTime = 0f;

    private void OnEnable()
    {
        spawnTime = Time.time;
        nextDamageTime = 0f;
    }

    private void Update()
    {
        // Despawn timer
        if (despawnAfterSeconds > 0f && Time.time >= spawnTime + despawnAfterSeconds)
        {
            if (debugLog) Debug.Log("[GhostEnemy] Despawn (timer).");
            Destroy(gameObject);
            return;
        }

        // Idle wenn kein Ziel
        if (!hasTarget)
            return;

        Vector2 pos = transform.position;
        Vector2 next = Vector2.MoveTowards(pos, targetPos, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(next.x, next.y, transform.position.z);

        if (Vector2.Distance(next, targetPos) <= arriveDistance)
        {
            hasTarget = false;
            if (debugLog) Debug.Log("[GhostEnemy] Arrived at target.");
        }
    }

    public void SetTarget(Vector2 worldPos)
    {
        targetPos = worldPos;
        hasTarget = true;

        if (debugLog) Debug.Log($"[GhostEnemy] Target set: {targetPos}");
    }

    public void ClearTarget()
    {
        hasTarget = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 1) PlayerHealth im Parent suchen (HurtBox/Feet/whatever ist egal)
        PlayerHealth hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) return;

        // 2) Optional: Root-Tag checken (nicht Child-Tag!)
        if (!string.IsNullOrEmpty(requiredRootTag))
        {
            Transform root = hp.transform; // PlayerHealth sitzt am Root (bei dir)
            if (root != null && !root.CompareTag(requiredRootTag))
                return;
        }

        // 3) Tick-Schaden (kein Frame-Spam)
        if (Time.time < nextDamageTime) return;
        nextDamageTime = Time.time + Mathf.Max(0.05f, damageInterval);

        hp.Damage(damagePerTick);

        if (debugLog) Debug.Log($"[GhostEnemy] Damage {damagePerTick} to Player.");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawDebugTarget) return;
        if (!hasTarget) return;

        Gizmos.DrawWireSphere(targetPos, 0.15f);
        Gizmos.DrawLine(transform.position, (Vector3)targetPos);
    }
#endif
}
