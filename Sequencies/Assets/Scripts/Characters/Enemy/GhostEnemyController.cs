/*using System.Collections;
using UnityEngine;

public class GhostEnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Lifetime")]
    [Tooltip("Ghost despawnt nach X Sekunden automatisch.")]
    [SerializeField] private float despawnAfterSeconds = 3f;

    [Tooltip("Wenn true: bei jedem neuen Target wird der Despawn-Timer zurückgesetzt.")]
    [SerializeField] private bool resetLifetimeOnRetarget = true;

    [Header("Fade (optional)")]
    [Tooltip("Wenn true: Ghost fadet beim Spawnen/Retarget IN (0->1) und fadet OUT kurz vor Despawn.")]
    [SerializeField] private bool useFade = true;

    [Tooltip("Dauer für Fade In/Out (Sekunden).")]
    [SerializeField] private float fadeTime = 0.25f;

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

    // Fade support
    private SpriteRenderer[] spriteRenderers;
    private Coroutine fadeInRoutine;
    private float alpha = 1f;

    // Enemy health
    private EnemyHealth enemyHealth;

    // -----------------------------------------------------
    // Spawner registration
    // -----------------------------------------------------
    public void RegisterSpawner(GhostSpawner owner)
    {
        spawner = owner;
    }

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
        nextDamageTime = 0f;

        if (useFade)
            StartFadeIn();
        else
            SetAlpha(1f);
    }

    private void Update()
    {
        UpdateFadeOutWindow();

        // Despawn timer
        if (despawnAfterSeconds > 0f && Time.time >= spawnTime + despawnAfterSeconds)
        {
            if (debugLog) Debug.Log("[GhostEnemy] Despawn (timer).");
            Despawn();
            return;
        }

        if (!hasTarget)
            return;

        Vector2 pos = transform.position;
        Vector2 next = Vector2.MoveTowards(pos, targetPos, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(next.x, next.y, transform.position.z);
    }

    public void SetTarget(Vector2 worldPos)
    {
        targetPos = worldPos;
        hasTarget = true;

        if (resetLifetimeOnRetarget)
            spawnTime = Time.time;

        if (useFade)
            StartFadeIn();
        else
            SetAlpha(1f);

        if (debugLog) Debug.Log($"[GhostEnemy] Target set: {targetPos} (timer reset={resetLifetimeOnRetarget})");
    }

    public void ClearTarget()
    {
        hasTarget = false;
    }

    // ----------------------------
    // Fade logic
    // ----------------------------
    private void StartFadeIn()
    {
        float ft = Mathf.Max(0.01f, fadeTime);

        if (fadeInRoutine != null)
        {
            StopCoroutine(fadeInRoutine);
            fadeInRoutine = null;
        }

        SetAlpha(0f);
        fadeInRoutine = StartCoroutine(FadeTo(1f, ft));
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            SetAlpha(Mathf.Lerp(start, target, k));
            yield return null;
        }

        SetAlpha(target);
        fadeInRoutine = null;
    }

    private void UpdateFadeOutWindow()
    {
        if (!useFade) return;
        if (despawnAfterSeconds <= 0f) return;
        if (fadeInRoutine != null) return;

        float ft = Mathf.Max(0.01f, fadeTime);
        float despawnAt = spawnTime + despawnAfterSeconds;
        float timeLeft = despawnAt - Time.time;

        if (timeLeft <= ft)
        {
            float a = Mathf.Clamp01(timeLeft / ft);
            SetAlpha(a);
        }
        else
        {
            if (alpha != 1f)
                SetAlpha(1f);
        }
    }

    private void SetAlpha(float a)
    {
        alpha = Mathf.Clamp01(a);

        if (spriteRenderers == null) return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;

            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private void Despawn()
    {
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
            if (root != null && !root.CompareTag(requiredRootTag))
                return;
        }

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
using System.Collections;
using UnityEngine;

public class GhostEnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Lifetime")]
    [Tooltip("Ghost despawnt nach X Sekunden automatisch.")]
    [SerializeField] private float despawnAfterSeconds = 3f;

    [Tooltip("Wenn true: bei jedem neuen Target wird der Despawn-Timer zurückgesetzt.")]
    [SerializeField] private bool resetLifetimeOnRetarget = true;

    [Header("Fade (optional)")]
    [Tooltip("Wenn true: Ghost fadet beim Spawnen/Retarget IN (0->1) und fadet OUT kurz vor Despawn.")]
    [SerializeField] private bool useFade = true;

    [Tooltip("Dauer für Fade In/Out (Sekunden).")]
    [SerializeField] private float fadeTime = 0.25f;

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

    // Fade support
    private SpriteRenderer[] spriteRenderers;
    private float[] baseAlphas;              // NEW: remembers original alpha per renderer (aura can be 0.18 etc.)
    private Coroutine fadeInRoutine;
    private float alpha = 1f;

    // Enemy health
    private EnemyHealth enemyHealth;

    // -----------------------------------------------------
    // Spawner registration
    // -----------------------------------------------------
    public void RegisterSpawner(GhostSpawner owner)
    {
        spawner = owner;
    }

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        enemyHealth = GetComponent<EnemyHealth>();

        // NEW: cache base alpha per renderer so inspector transparency stays intact
        if (spriteRenderers != null)
        {
            baseAlphas = new float[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                var sr = spriteRenderers[i];
                baseAlphas[i] = (sr != null) ? sr.color.a : 1f;
            }
        }
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
        nextDamageTime = 0f;

        if (useFade)
            StartFadeIn();
        else
            SetAlpha(1f);
    }

    private void Update()
    {
        UpdateFadeOutWindow();

        // Despawn timer
        if (despawnAfterSeconds > 0f && Time.time >= spawnTime + despawnAfterSeconds)
        {
            if (debugLog) Debug.Log("[GhostEnemy] Despawn (timer).");
            Despawn();
            return;
        }

        if (!hasTarget)
            return;

        Vector2 pos = transform.position;
        Vector2 next = Vector2.MoveTowards(pos, targetPos, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(next.x, next.y, transform.position.z);
    }

    public void SetTarget(Vector2 worldPos)
    {
        targetPos = worldPos;
        hasTarget = true;

        if (resetLifetimeOnRetarget)
            spawnTime = Time.time;

        if (useFade)
            StartFadeIn();
        else
            SetAlpha(1f);

        if (debugLog) Debug.Log($"[GhostEnemy] Target set: {targetPos} (timer reset={resetLifetimeOnRetarget})");
    }

    public void ClearTarget()
    {
        hasTarget = false;
    }

    // ----------------------------
    // Fade logic
    // ----------------------------
    private void StartFadeIn()
    {
        float ft = Mathf.Max(0.01f, fadeTime);

        if (fadeInRoutine != null)
        {
            StopCoroutine(fadeInRoutine);
            fadeInRoutine = null;
        }

        SetAlpha(0f);
        fadeInRoutine = StartCoroutine(FadeTo(1f, ft));
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            SetAlpha(Mathf.Lerp(start, target, k));
            yield return null;
        }

        SetAlpha(target);
        fadeInRoutine = null;
    }

    private void UpdateFadeOutWindow()
    {
        if (!useFade) return;
        if (despawnAfterSeconds <= 0f) return;
        if (fadeInRoutine != null) return;

        float ft = Mathf.Max(0.01f, fadeTime);
        float despawnAt = spawnTime + despawnAfterSeconds;
        float timeLeft = despawnAt - Time.time;

        if (timeLeft <= ft)
        {
            float a = Mathf.Clamp01(timeLeft / ft);
            SetAlpha(a);
        }
        else
        {
            if (alpha != 1f)
                SetAlpha(1f);
        }
    }

    // NEW: baseAlpha * fadeAlpha, so aura transparency works
    private void SetAlpha(float a)
    {
        alpha = Mathf.Clamp01(a);

        if (spriteRenderers == null) return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;

            float baseA = 1f;
            if (baseAlphas != null && i < baseAlphas.Length)
                baseA = baseAlphas[i];

            Color c = sr.color;
            c.a = baseA * alpha;
            sr.color = c;
        }
    }

    private void Despawn()
    {
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
            if (root != null && !root.CompareTag(requiredRootTag))
                return;
        }

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
