using UnityEngine;
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
}
