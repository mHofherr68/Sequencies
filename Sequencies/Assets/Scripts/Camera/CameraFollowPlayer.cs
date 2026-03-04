using UnityEngine;

/// <summary>
/// Simple camera follow behaviour.
/// 
/// The camera smoothly follows a target Transform (typically the player)
/// using a damped movement to avoid hard snapping.
/// 
/// The camera only follows the X and Y position of the target,
/// while keeping its original Z position.
/// Movement smoothing is handled using Vector3.SmoothDamp.
/// </summary>
public class CameraFollowPlayer : MonoBehaviour
{
    [Tooltip("Target transform that the camera should follow (usually the player).")]
    [SerializeField] private Transform target;

    [Tooltip("Time required for the camera to smooth towards the target position.")]
    [SerializeField] private float smoothTime = 0.12f;

    /// <summary>
    /// Internal velocity reference used by SmoothDamp.
    /// This value is automatically updated every frame.
    /// </summary>
    private Vector3 velocity;

    /// <summary>
    /// Called after all Update methods have finished.
    /// 
    /// LateUpdate is used for camera movement to ensure
    /// the player has already moved during the frame,
    /// preventing jitter.
    /// </summary>
    private void LateUpdate()
    {
        if (!target) return;

        // Desired camera position following the target's X/Y
        // while preserving the camera's current Z position.
        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);

        // Smoothly interpolate camera position toward the target.
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
