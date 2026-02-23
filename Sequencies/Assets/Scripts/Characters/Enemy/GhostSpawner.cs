/*using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject ghostPrefab;

    [Header("Spawn")]
    [SerializeField] private float spawnRadius = 6f;
    [SerializeField] private int maxTries = 20;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    public void SpawnGhostTo(Vector2 targetPos)
    {
        if (ghostPrefab == null) return;

        Vector2 spawnPos = FindSpawnPosNear(targetPos);

        GameObject go = Instantiate(ghostPrefab, spawnPos, Quaternion.identity);
        var ghost = go.GetComponent<GhostEnemyController>();
        if (ghost != null)
            ghost.SetTarget(targetPos);

        if (debugLog) Debug.Log($"[GhostSpawner] Spawn @ {spawnPos} -> Target {targetPos}");
    }

    private Vector2 FindSpawnPosNear(Vector2 around)
    {
        // Simple & safe: random around target. Later you can bias to "dark".
        for (int i = 0; i < maxTries; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector2 p = around + offset;

            // If you want: keep within level bounds here.
            return p;
        }

        return around + Vector2.right * spawnRadius;
    }
}*/
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject ghostPrefab;

    [Header("Spawn")]
    [Tooltip("Ghost spawn radius um die Zielposition herum.")]
    [SerializeField] private float spawnRadius = 6f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

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
}
