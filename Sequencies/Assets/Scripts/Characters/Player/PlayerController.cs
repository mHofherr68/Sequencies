/*using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private PlayerAnimationController anim;

    public Vector2 CurrentVelocity { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<PlayerAnimationController>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        // Animator sofort informieren
        anim.SetAnimatorValues(moveInput.x, moveInput.y);
    }

    private void FixedUpdate()
    {
        CurrentVelocity = moveInput * moveSpeed;

        Vector2 next = rb.position + CurrentVelocity * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }
}*/
/*using UnityEngine;                  // With clap Sound via FX_SoundSystem
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Clap")]
    [SerializeField] private float clapCooldown = 0.6f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerAnimationController anim;

    public Vector2 CurrentVelocity { get; private set; }

    private float lastClapTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<PlayerAnimationController>();
    }

    // INPUT SYSTEM ? Move
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        anim.SetAnimatorValues(moveInput.x, moveInput.y);
    }

    // INPUT SYSTEM ? Clap (Space)
    public void OnClap(InputValue value)
    {
        if (!value.isPressed) return;

        if (Time.time < lastClapTime + clapCooldown)
            return;

        lastClapTime = Time.time;

        // FIXED TAG
        FX_SoundSystem.I?.PlayHit("Clap", true);
    }

    private void FixedUpdate()
    {
        CurrentVelocity = moveInput * moveSpeed;

        Vector2 next = rb.position + CurrentVelocity * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }
}*/
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
    [SerializeField] private GhostSpawner ghostSpawner;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerAnimationController anim;

    public Vector2 CurrentVelocity { get; private set; }

    private float lastClapTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<PlayerAnimationController>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        anim.SetAnimatorValues(moveInput.x, moveInput.y);
    }

    public void OnClap(InputValue value)
    {
        if (!value.isPressed) return;

        if (Time.time < lastClapTime + clapCooldown)
            return;

        lastClapTime = Time.time;

        // Sound
        FX_SoundSystem.I?.PlayHit("Clap", true);

        // Spawn + send ghost to clap position (ONLY on clap)
        if (ghostSpawner != null)
            ghostSpawner.SpawnGhostTo(transform.position);
    }

    private void FixedUpdate()
    {
        CurrentVelocity = moveInput * moveSpeed;

        Vector2 next = rb.position + CurrentVelocity * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }
}