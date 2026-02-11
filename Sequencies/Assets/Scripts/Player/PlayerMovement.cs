using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    public Vector2 CurrentVelocity { get; private set; }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        CurrentVelocity = moveInput * moveSpeed;

        Vector2 next = rb.position + CurrentVelocity * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }
}
