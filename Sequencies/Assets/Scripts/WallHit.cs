using UnityEngine;

public class WallHit : MonoBehaviour
{
    [Header("What should this wall report to ThrowableFlight?")]
    [SerializeField] private LayerMask reportedWallMask;

    public LayerMask ReportedWallMask => reportedWallMask;
}