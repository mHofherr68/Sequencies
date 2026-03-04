using UnityEngine;

/// <summary>
/// Handles player animation parameters based on movement input.
/// Updates the Animator with directional movement values.
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    /// <summary>
    /// Reference to the Animator component controlling player animations.
    /// </summary>
    private Animator animator;

    /// <summary>
    /// Retrieves the Animator component on startup.
    /// </summary>
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Updates movement parameters in the Animator.
    /// These values control directional animation blending.
    /// </summary>
    /// <param name="moveX">Horizontal movement input.</param>
    /// <param name="moveY">Vertical movement input.</param>
    public void SetAnimatorValues(float moveX, float moveY)
    {
        animator.SetFloat("moveX", moveX);
        animator.SetFloat("moveY", moveY);
    }
}