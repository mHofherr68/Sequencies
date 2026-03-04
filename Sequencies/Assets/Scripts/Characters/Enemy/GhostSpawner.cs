using UnityEngine;

/// <summary>
/// Spawns and manages a single ghost enemy instance.
/// 
/// - Singleton (GhostSpawner.I) and persistent across scenes.
/// - "Soft lock": if a ghost is already active, it will not spawn a second one,
///   instead it retargets the active ghost to the new position.
/// - The active ghost notifies this spawner when it despawns.
/// </summary>
public class GhostSpawner : MonoBehaviour
{
    /// <summary>Global singleton instance.</summary>
    public static GhostSpawner I { get; private set; }

    [Header("Prefab")]

    [Tooltip("Ghost prefab to spawn.")]
    [SerializeField] private GameObject ghostPrefab;

    [Header("Spawn")]

    [Tooltip("Spawn radius around the target position (randomized).")]
    [SerializeField] private float spawnRadius = 6f;

    [Header("Debug")]

    [Tooltip("If true, logs spawner actions to the console.")]
    [SerializeField] private bool debugLog = false;

    /// <summary>
    /// Soft-lock reference: only one ghost can be active at a time.
    /// If this is not null, SpawnGhostTo() will retarget instead of spawning.
    /// </summary>
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

        // Persist the root object across scene loads
        DontDestroyOnLoad(transform.root.gameObject);
    }

    /// <summary>
    /// Spawns a new ghost near the target position, or retargets the currently active ghost.
    /// </summary>
    /// <param name="targetPos">World position the ghost should move towards.</param>
    public void SpawnGhostTo(Vector2 targetPos)
    {
        if (ghostPrefab == null)
        {
            if (debugLog)
                Debug.LogWarning("[GhostSpawner] ghostPrefab is NULL.");
            return;
        }

        // -----------------------------------------------------
        // Soft lock: only one ghost instance at a time
        // -----------------------------------------------------
        if (activeGhost != null)
        {
            if (debugLog)
                Debug.Log("[GhostSpawner] Ghost already active -> updating target.");

            activeGhost.SetTarget(targetPos);
            return;
        }

        // -----------------------------------------------------
        // Spawn a new ghost instance near the target
        // -----------------------------------------------------
        Vector2 spawnPos =
            targetPos + Random.insideUnitCircle * Mathf.Max(0f, spawnRadius);

        GameObject go =
            Instantiate(ghostPrefab, spawnPos, Quaternion.identity);

        activeGhost = go.GetComponent<GhostEnemyController>();

        if (activeGhost != null)
        {
            // Assign initial target and register back-reference for despawn notification
            activeGhost.SetTarget(targetPos);
            activeGhost.RegisterSpawner(this);
        }

        if (debugLog)
            Debug.Log($"[GhostSpawner] Spawn @ {spawnPos} -> Target {targetPos}");
    }

    /// <summary>
    /// Called by the active ghost when it despawns.
    /// Clears the soft-lock reference so a new ghost can spawn later.
    /// </summary>
    /// <param name="ghost">Ghost instance that is despawning.</param>
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