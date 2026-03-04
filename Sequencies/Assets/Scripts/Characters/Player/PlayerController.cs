using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player movement, clap interaction, and ghost spawning.
/// 
/// Responsibilities:
/// - Handles player movement using Rigidbody2D.
/// - Sends movement values to the PlayerAnimationController.
/// - Triggers clap actions (sound, optional light, optional ghost spawn).
/// - Provides a global movement lock used by doors, death, or cutscenes.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]

    [Tooltip("Player movement speed.")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Clap")]

    [Tooltip("Cooldown time between clap actions.")]
    [SerializeField] private float clapCooldown = 0.6f;

    [Header("Ghost (spawn on clap / noise)")]

    [Tooltip("Optional reference to the GhostSpawner. If empty, GhostSpawner.I will be used.")]
    [SerializeField] private GhostSpawner ghostSpawner;

    [Tooltip("If enabled, clap and noise events are allowed to spawn a ghost.")]
    [SerializeField] private bool enableGhostSpawning = true;

    [Header("Noise Light (optional)")]

    [Tooltip("Prefab containing a Point Light 2D. Spawned at the player position when clapping.")]
    [SerializeField] private GameObject noiseLightPrefab;

    [Tooltip("If true, a permanent noise light is spawned when the player claps.")]
    [SerializeField] private bool spawnLightOnClap = true;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerAnimationController anim;

    /// <summary>
    /// Current player movement velocity (world space).
    /// </summary>
    public Vector2 CurrentVelocity { get; private set; }

    /// <summary>
    /// Timestamp of the last clap action.
    /// </summary>
    private float lastClapTime = -999f;

    /// <summary>
    /// Global movement lock used by systems such as doors, death, or cutscenes.
    /// </summary>
    private bool movementEnabled = true;

    /// <summary>
    /// Indicates whether this player is allowed to trigger ghost spawning.
    /// </summary>
    public bool GhostSpawningEnabled => enableGhostSpawning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<PlayerAnimationController>();

        // Resolve persistent GhostSpawner instance if none assigned
        if (ghostSpawner == null)
            ghostSpawner = GhostSpawner.I;
    }

    // =====================================================
    // Movement Lock API
    // =====================================================

    /// <summary>
    /// Enables or disables player movement globally.
    /// Used by external systems (doors, cutscenes, death, etc.).
    /// </summary>
    /// <param name="enabled">True = movement allowed, False = movement disabled.</param>
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!enabled)
        {
            moveInput = Vector2.zero;
            CurrentVelocity = Vector2.zero;

            rb.linearVelocity = Vector2.zero;

            anim.SetAnimatorValues(0f, 0f);
        }
    }

    /// <summary>
    /// Returns whether player movement is currently enabled.
    /// </summary>
    public bool IsMovementEnabled => movementEnabled;

    // =====================================================
    // Input System callbacks (PlayerInput -> Send Messages)
    // =====================================================

    /// <summary>
    /// Handles movement input from the Unity Input System.
    /// Updates movement direction and animation parameters.
    /// </summary>
    /// <param name="value">Input value containing the movement vector.</param>
    public void OnMove(InputValue value)
    {
        if (!movementEnabled) return;

        moveInput = value.Get<Vector2>();
        anim.SetAnimatorValues(moveInput.x, moveInput.y);
    }

    /// <summary>
    /// Handles the clap input action.
    /// Triggers a sound, optional noise light, and optional ghost spawn.
    /// </summary>
    /// <param name="value">Input value indicating button state.</param>
    public void OnClap(InputValue value)
    {
        if (!movementEnabled) return;
        if (!value.isPressed) return;

        // Clap cooldown check
        if (Time.time < lastClapTime + clapCooldown)
            return;

        lastClapTime = Time.time;

        // Play clap sound
        FX_SoundSystem.I?.PlayHit("Clap", true);

        // Spawn noise light at player position
        if (spawnLightOnClap && noiseLightPrefab != null)
        {
            Vector3 p = transform.position;
            p.z = 0f;
            Instantiate(noiseLightPrefab, p, Quaternion.identity);
        }

        // Spawn ghost if allowed
        if (!enableGhostSpawning) return;

        GhostSpawner spawner = ghostSpawner != null ? ghostSpawner : GhostSpawner.I;
        if (spawner != null)
            spawner.SpawnGhostTo(transform.position);
    }

    /// <summary>
    /// Handles player movement using Rigidbody2D physics.
    /// </summary>
    private void FixedUpdate()
    {
        if (!movementEnabled) return;

        CurrentVelocity = moveInput * moveSpeed;

        Vector2 next = rb.position + CurrentVelocity * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }
}