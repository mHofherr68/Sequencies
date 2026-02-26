using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Clap")]
    [SerializeField] private float clapCooldown = 0.6f;

    [Header("Ghost (spawn on clap)")]
    [Tooltip("Optional: im Inspector setzen. Wenn leer, nutzt er GhostSpawner.I (persistent).")]
    [SerializeField] private GhostSpawner ghostSpawner;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerAnimationController anim;

    public Vector2 CurrentVelocity { get; private set; }

    private float lastClapTime = -999f;

    // global movement lock (used by doors, death, cutscenes, etc.)
    private bool movementEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<PlayerAnimationController>();

        // NEW: auto-resolve persistent spawner if inspector ref is missing (Level 2/3 case)
        if (ghostSpawner == null)
            ghostSpawner = GhostSpawner.I;
    }

    // =====================================================
    // Movement lock API
    // =====================================================
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

    public bool IsMovementEnabled => movementEnabled;

    // =====================================================
    // Input System callbacks (PlayerInput -> Send Messages)
    // =====================================================
    public void OnMove(InputValue value)
    {
        if (!movementEnabled) return;

        moveInput = value.Get<Vector2>();
        anim.SetAnimatorValues(moveInput.x, moveInput.y);
    }

    public void OnClap(InputValue value)
    {
        if (!movementEnabled) return;
        if (!value.isPressed) return;

        if (Time.time < lastClapTime + clapCooldown)
            return;

        lastClapTime = Time.time;

        FX_SoundSystem.I?.PlayHit("Clap", true);

        // NEW: robust spawner selection (works across scenes)
        GhostSpawner spawner = ghostSpawner != null ? ghostSpawner : GhostSpawner.I;
        if (spawner != null)
            spawner.SpawnGhostTo(transform.position);
    }

    private void FixedUpdate()
    {
        if (!movementEnabled) return;

        CurrentVelocity = moveInput * moveSpeed;

        Vector2 next = rb.position + CurrentVelocity * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }
}
