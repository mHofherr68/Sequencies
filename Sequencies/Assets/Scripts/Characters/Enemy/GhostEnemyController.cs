using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the Ghost enemy movement, lifetime, fade visuals, and player damage.
/// 
/// Responsibilities:
/// - Moves the ghost towards a target position (SetTarget).
/// - Despawns automatically after a configured lifetime.
/// - Optionally resets the despawn timer when retargeted.
/// - Optional fade-in on spawn/retarget and fade-out shortly before despawn.
/// - Damages the player at fixed intervals while the player stays inside the ghost trigger collider.
/// - Preserves per-renderer inspector alpha values (e.g., aura sprites) while applying global fade.
/// 
/// Notes:
/// - The GhostSpawner reference is registered by the spawner (RegisterSpawner) so the ghost can notify it on despawn.
/// - EnemyHealth is referenced but not used in this current version (kept for integration with enemy HP systems).
/// </summary>
public class GhostEnemyController : MonoBehaviour
{
    [Header("Movement")]

    [Tooltip("Movement speed used when moving towards the current target position.")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Lifetime")]

    [Tooltip("The ghost despawns automatically after this many seconds. Set to 0 to disable the lifetime timer.")]
    [SerializeField] private float despawnAfterSeconds = 3f;

    [Tooltip("If true, the despawn timer is reset whenever a new target is assigned via SetTarget().")]
    [SerializeField] private bool resetLifetimeOnRetarget = true;

    [Header("Fade (optional)")]

    [Tooltip("If true, the ghost fades IN on spawn/retarget (0?1) and fades OUT shortly before despawn.")]
    [SerializeField] private bool useFade = true;

    [Tooltip("Fade duration in seconds (used for fade in and for the final fade-out window).")]
    [SerializeField] private float fadeTime = 0.25f;

    [Header("Player Damage")]

    [Tooltip("Damage dealt per tick while the player is within the ghost trigger collider (e.g. 10 = 10% if max HP is 100).")]
    [SerializeField] private int damagePerTick = 10;

    [Tooltip("Time between damage ticks while the player stays in contact with the ghost.")]
    [SerializeField] private float damageInterval = 0.75f;

    [Header("Optional Tag Filter (Root Tag)")]

    [Tooltip("If empty: no tag check. If set: the PlayerHealth root object must have this tag (e.g. 'Player').")]
    [SerializeField] private string requiredRootTag = "Player";

    [Header("Debug")]

    [Tooltip("If true, prints ghost lifecycle and combat events to the console.")]
    [SerializeField] private bool debugLog = false;

    [Tooltip("If true, draws debug gizmos for the current target position (Editor only).")]
    [SerializeField] private bool drawDebugTarget = true;

    private Vector2 targetPos;
    private bool hasTarget;

    private float spawnTime;
    private float nextDamageTime = 0f;

    /// <summary>
    /// Reference back to the spawner that owns this ghost.
    /// Used to notify the spawner when the ghost despawns.
    /// </summary>
    private GhostSpawner spawner;

    // Fade support
    private SpriteRenderer[] spriteRenderers;

    /// <summary>
    /// Stores the original alpha of each SpriteRenderer so inspector transparency stays intact.
    /// For example, aura sprites can keep a base alpha of 0.18 while still fading globally.
    /// </summary>
    private float[] baseAlphas;

    private Coroutine fadeInRoutine;

    /// <summary>
    /// Global fade alpha multiplier (0..1). Applied on top of baseAlphas[].
    /// </summary>
    private float alpha = 1f;

    /// <summary>
    /// Optional reference to the enemy health component.
    /// Present for integration with systems that reduce ghost HP.
    /// </summary>
    private EnemyHealth enemyHealth;

    // -----------------------------------------------------
    // Spawner registration
    // -----------------------------------------------------

    /// <summary>
    /// Registers the spawner that created/owns this ghost instance.
    /// The ghost notifies the spawner when it despawns so the spawner can allow future spawns.
    /// </summary>
    /// <param name="owner">Spawner instance that owns this ghost.</param>
    public void RegisterSpawner(GhostSpawner owner)
    {
        spawner = owner;
    }

    /// <summary>
    /// Caches SpriteRenderers (including inactive children) and stores their original alpha values.
    /// This ensures inspector-defined transparency (e.g. aura sprites) remains correct during fades.
    /// Also resolves EnemyHealth on the same GameObject (if present).
    /// </summary>
    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        enemyHealth = GetComponent<EnemyHealth>();

        // Cache base alpha per renderer so inspector transparency stays intact
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

    /// <summary>
    /// Resets the lifetime timer and starts a fade-in (if enabled) when the ghost becomes active.
    /// </summary>
    private void OnEnable()
    {
        spawnTime = Time.time;
        nextDamageTime = 0f;

        if (useFade)
            StartFadeIn();
        else
            SetAlpha(1f);
    }

    /// <summary>
    /// Updates fade-out window, despawn timer, and movement towards target position.
    /// </summary>
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

        // Idle if no target is active
        if (!hasTarget)
            return;

        // Move towards target position
        Vector2 pos = transform.position;
        Vector2 next = Vector2.MoveTowards(pos, targetPos, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(next.x, next.y, transform.position.z);
    }

    /// <summary>
    /// Assigns a new target world position and enables movement towards it.
    /// Optionally resets lifetime timer and triggers a fade-in on retarget.
    /// </summary>
    /// <param name="worldPos">Target world position for the ghost to move towards.</param>
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

    /// <summary>
    /// Stops movement and clears the current target state.
    /// </summary>
    public void ClearTarget()
    {
        hasTarget = false;
    }

    // ----------------------------
    // Fade logic
    // ----------------------------

    /// <summary>
    /// Starts a fade-in (0 ? 1) over fadeTime seconds.
    /// Cancels any running fade-in routine before starting a new one.
    /// </summary>
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

    /// <summary>
    /// Coroutine that interpolates the global fade alpha to a target value over time.
    /// </summary>
    /// <param name="target">Target alpha (0..1).</param>
    /// <param name="duration">Interpolation duration in seconds.</param>
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

    /// <summary>
    /// Applies fade-out only during the final fadeTime window before despawn.
    /// Outside this window, the ghost remains fully visible (alpha = 1) unless a fade-in is running.
    /// </summary>
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

    /// <summary>
    /// Sets the global fade alpha (0..1) and applies it to all SpriteRenderers.
    /// 
    /// Implementation detail:
    /// The applied alpha is (baseAlpha * fadeAlpha) so that aura sprites can keep
    /// their inspector-defined transparency while still being affected by fading.
    /// </summary>
    /// <param name="a">Fade alpha multiplier (0..1).</param>
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

    /// <summary>
    /// Despawns the ghost, notifies the spawner, and destroys the instance.
    /// </summary>
    private void Despawn()
    {
        spawner?.NotifyGhostDespawn(this);
        Destroy(gameObject);
    }

    /// <summary>
    /// Damages the player while the player collider stays inside the ghost trigger.
    /// Damage is applied in intervals to avoid per-frame spam.
    /// </summary>
    /// <param name="other">Collider that is currently inside the trigger.</param>
    private void OnTriggerStay2D(Collider2D other)
    {
        // 1) Find PlayerHealth in parents (works with HurtBox/Feet/etc.)
        PlayerHealth hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) return;

        // 2) Optional: validate root tag (root is where PlayerHealth is placed)
        if (!string.IsNullOrEmpty(requiredRootTag))
        {
            Transform root = hp.transform;
            if (root != null && !root.CompareTag(requiredRootTag))
                return;
        }

        // 3) Tick-based damage (no frame spam)
        if (Time.time < nextDamageTime) return;
        nextDamageTime = Time.time + Mathf.Max(0.05f, damageInterval);

        hp.Damage(damagePerTick);

        if (debugLog) Debug.Log($"[GhostEnemy] Damage {damagePerTick} to Player.");
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only debug gizmos for the current target position.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!drawDebugTarget) return;
        if (!hasTarget) return;

        Gizmos.DrawWireSphere(targetPos, 0.15f);
        Gizmos.DrawLine(transform.position, (Vector3)targetPos);
    }
#endif
}