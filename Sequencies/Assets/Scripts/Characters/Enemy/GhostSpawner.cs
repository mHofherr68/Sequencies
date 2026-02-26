/*using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    public static GhostSpawner I { get; private set; }

    [Header("Prefab")]
    [SerializeField] private GameObject ghostPrefab;

    [Header("Spawn")]
    [Tooltip("Ghost spawn radius um die Zielposition herum.")]
    [SerializeField] private float spawnRadius = 6f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private void Awake()
    {
        // Singleton guard
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        // IMPORTANT: persist ROOT, not a child
        DontDestroyOnLoad(transform.root.gameObject);
    }

    public void SpawnGhostTo(Vector2 targetPos)
    {
        if (ghostPrefab == null)
        {
            if (debugLog) Debug.LogWarning("[GhostSpawner] ghostPrefab is NULL.");
            return;
        }

        Vector2 spawnPos = targetPos + Random.insideUnitCircle * Mathf.Max(0f, spawnRadius);

        GameObject go = Instantiate(ghostPrefab, spawnPos, Quaternion.identity);

        var ghost = go.GetComponent<GhostEnemyController>();
        if (ghost != null)
            ghost.SetTarget(targetPos);

        if (debugLog) Debug.Log($"[GhostSpawner] Spawn @ {spawnPos} -> Target {targetPos}");
    }
}*/
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    public static GhostSpawner I { get; private set; }

    [Header("Prefab")]
    [SerializeField] private GameObject ghostPrefab;

    [Header("Spawn")]
    [Tooltip("Ghost spawn radius um die Zielposition herum.")]
    [SerializeField] private float spawnRadius = 6f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    // ? Soft-Lock reference
    private GhostEnemyController activeGhost;

    private void Awake()
    {
        // Singleton guard
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        // persist ROOT object
        DontDestroyOnLoad(transform.root.gameObject);
    }

    public void SpawnGhostTo(Vector2 targetPos)
    {
        if (ghostPrefab == null)
        {
            if (debugLog)
                Debug.LogWarning("[GhostSpawner] ghostPrefab is NULL.");
            return;
        }

        // =====================================================
        // ? SOFT LOCK
        // =====================================================
        if (activeGhost != null)
        {
            if (debugLog)
                Debug.Log("[GhostSpawner] Ghost already active -> updating target.");

            activeGhost.SetTarget(targetPos);
            return;
        }

        // =====================================================
        // Spawn NEW ghost
        // =====================================================
        Vector2 spawnPos =
            targetPos + Random.insideUnitCircle * Mathf.Max(0f, spawnRadius);

        GameObject go =
            Instantiate(ghostPrefab, spawnPos, Quaternion.identity);

        activeGhost = go.GetComponent<GhostEnemyController>();

        if (activeGhost != null)
        {
            activeGhost.SetTarget(targetPos);

            // ? inform spawner when ghost dies
            activeGhost.RegisterSpawner(this);
        }

        if (debugLog)
            Debug.Log($"[GhostSpawner] Spawn @ {spawnPos} -> Target {targetPos}");
    }

    // called by Ghost when despawned
    public void NotifyGhostDespawn(GhostEnemyController ghost)
    {
        if (ghost == activeGhost)
        {
            if (debugLog)
                Debug.Log("[GhostSpawner] Ghost cleared.");

            activeGhost = null;
        }
    }
}
