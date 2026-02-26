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
}*/
/*using UnityEngine;        // Richtungswechsel

public class GhostEnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float arriveDistance = 0.15f;

    [Header("Lifetime")]
    [SerializeField] private float despawnAfterSeconds = 3f;

    [Header("Player Damage")]
    [SerializeField] private int damagePerTick = 10;
    [SerializeField] private float damageInterval = 0.75f;

    [Header("Optional Tag Filter (Root Tag)")]
    [SerializeField] private string requiredRootTag = "Player";

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;
    [SerializeField] private bool drawDebugTarget = true;

    private Vector2 targetPos;
    private bool hasTarget;

    private float spawnTime;
    private float nextDamageTime;

    // ? reference back to spawner
    private GhostSpawner spawner;

    // -----------------------------------------------------
    // Spawner registration
    // -----------------------------------------------------
    public void RegisterSpawner(GhostSpawner owner)
    {
        spawner = owner;
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
        nextDamageTime = 0f;
    }

    private void Update()
    {
        // =============================
        // Lifetime despawn
        // =============================
        if (despawnAfterSeconds > 0f &&
            Time.time >= spawnTime + despawnAfterSeconds)
        {
            Despawn();
            return;
        }

        if (!hasTarget)
            return;

        Vector2 pos = transform.position;

        Vector2 next =
            Vector2.MoveTowards(pos, targetPos, moveSpeed * Time.deltaTime);

        transform.position =
            new Vector3(next.x, next.y, transform.position.z);

        if (Vector2.Distance(next, targetPos) <= arriveDistance)
        {
            hasTarget = false;

            if (debugLog)
                Debug.Log("[GhostEnemy] Arrived at target.");
        }
    }

    public void SetTarget(Vector2 worldPos)
    {
        targetPos = worldPos;
        hasTarget = true;

        if (debugLog)
            Debug.Log($"[GhostEnemy] Target set: {targetPos}");
    }

    private void Despawn()
    {
        if (debugLog)
            Debug.Log("[GhostEnemy] Despawn.");

        spawner?.NotifyGhostDespawn(this);

        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) return;

        if (!string.IsNullOrEmpty(requiredRootTag))
        {
            Transform root = hp.transform;
            if (!root.CompareTag(requiredRootTag))
                return;
        }

        if (Time.time < nextDamageTime) return;

        nextDamageTime =
            Time.time + Mathf.Max(0.05f, damageInterval);

        hp.Damage(damagePerTick);

        if (debugLog)
            Debug.Log($"[GhostEnemy] Damage {damagePerTick}");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawDebugTarget || !hasTarget) return;

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

    [Tooltip("Wenn true: bei jedem neuen Target wird der Despawn-Timer zurückgesetzt.")]
    [SerializeField] private bool resetLifetimeOnRetarget = true;

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
    private float nextDamageTime = 0f;

    // reference back to spawner (soft-lock)
    private GhostSpawner spawner;

    // -----------------------------------------------------
    // Spawner registration
    // -----------------------------------------------------
    public void RegisterSpawner(GhostSpawner owner)
    {
        spawner = owner;
    }

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
            Despawn();
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

        // ? RESET lifetime when retargeting (so it doesn't despawn mid-chase)
        if (resetLifetimeOnRetarget)
            spawnTime = Time.time;

        if (debugLog) Debug.Log($"[GhostEnemy] Target set: {targetPos} (timer reset={resetLifetimeOnRetarget})");
    }

    public void ClearTarget()
    {
        hasTarget = false;
    }

    private void Despawn()
    {
        // tell spawner so it can allow next spawn
        spawner?.NotifyGhostDespawn(this);
        Destroy(gameObject);
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
