using UnityEngine;

/// <summary>
/// Handles the full flight simulation of a thrown item projectile:
/// - Moves the projectile from a start position to a target position (with optional arc).
/// - Handles collisions/planning against wall tilemaps (top view + side view) and a top barrier.
/// - Produces "noise" on impacts (SFX + optional permanent light + optional ghost retarget).
/// - Spawns an ItemPickup on landing (optional) and destroys the projectile.
/// </summary>
public class ThrowableFlight : MonoBehaviour
{
    // ============================================================
    // Impact / Detection
    // ============================================================

    [Header("Impact")]
    [Tooltip("Radius used to detect a surface MaterialHit when the projectile lands.")]
    [SerializeField] private float impactRadius = 0.25f;

    [Header("Detection")]
    [Tooltip("Layer mask used for detecting surfaces (MaterialHit / BreakableWindow, etc.) on landing.")]
    [SerializeField] private LayerMask surfaceMask = ~0;

    // TM_Wall_01 (TopView) - original
    [Header("Walls (Top View)")]
    [Tooltip("Layer mask for top-view walls (TM_Wall_01). Used for bounce planning and safe landing offsets.")]
    [SerializeField] private LayerMask wallMaskTopView = 0;

    // TM_Wall_02 (SideView) - face (tilemap)
    [Header("Walls (Side View)")]
    [Tooltip("Layer mask for side-view wall faces (TM_Wall_02). Used to plan the side-slide/drop behavior.")]
    [SerializeField] private LayerMask wallMaskSideView = 0;

    // TopCollider on TM_Wall_02 (child) to prevent throwing over from above
    [Tooltip("Layer mask for the top barrier collider that prevents throwing over the side-view wall.")]
    [SerializeField] private LayerMask wallTopBarier = 0; // set to WallTopBarier

    // ============================================================
    // Ghost + Noise
    // ============================================================

    [Header("Ghost Noise (from this projectile)")]
    [Tooltip("Optional. If not assigned, GhostSpawner.I (persistent singleton) is used.")]
    [SerializeField] private GhostSpawner ghostSpawner;

    [Tooltip("If true: projectile noise events (wall/surface) attempt to retarget/spawn the ghost (if the Player allows it).")]
    [SerializeField] private bool triggerGhostOnNoise = true;

    // Cached PlayerController reference used for the "GhostSpawningEnabled" gate.
    private PlayerController playerController;

    [Header("Noise Light (Static / optional)")]
    [Tooltip("Prefab containing a Point Light 2D (permanent). Spawned at every noise position.")]
    [SerializeField] private GameObject noiseLightStaticPrefab;

    [Tooltip("If true: projectile noise spawns a permanent light at the noise position.")]
    [SerializeField] private bool spawnStaticLightOnNoise = true;

    // ============================================================
    // Wall Bounce
    // ============================================================

    [Header("Wall Bounce")]
    [Tooltip("Small push distance applied away from a wall hit to avoid overlapping colliders.")]
    [SerializeField] private float wallDistance = 0.05f;

    [Tooltip("Random minimum distance for a bounce segment.")]
    [SerializeField] private float wallBounceDistanceMin = 0.25f;

    [Tooltip("Random maximum distance for a bounce segment.")]
    [SerializeField] private float wallBounceDistanceMax = 0.55f;

    [Tooltip("Initial speed used during a bounce segment.")]
    [SerializeField] private float wallBounceSpeed = 8f;

    [Tooltip("Drag factor applied during a bounce segment to reduce bounce speed over time.")]
    [SerializeField] private float wallBounceDrag = 4f;

    // ============================================================
    // SideWall Slide / Drop (TM_Wall_02)
    // ============================================================

    [Header("SideWall Slide (TM_Wall_02)")]
    [Tooltip("Downward speed while sliding along the side wall face collider.")]
    [SerializeField] private float sideSlideSpeed = 7.5f;

    [Tooltip("Minimum downward step per frame while sliding (prevents zero-step at low FPS).")]
    [SerializeField] private float sideSlideMinStep = 0.02f;

    [Header("SideWall Spiral Drop (after leaving collider)")]
    [Tooltip("Minimum drop distance after leaving the side wall collider.")]
    [SerializeField] private float sideSpiralDropDistanceMin = 0.45f;

    [Tooltip("Maximum drop distance after leaving the side wall collider.")]
    [SerializeField] private float sideSpiralDropDistanceMax = 1.10f;

    // ============================================================
    // Flight / Perceived Speed
    // ============================================================

    [Header("Perceived Speed (Up/Down Throws)")]
    [Tooltip("If true: applies a speed multiplier curve for vertical throws to feel more natural.")]
    [SerializeField] private bool easeOutOnVerticalThrows = true;

    [Tooltip("Speed multiplier at the beginning of a vertical throw.")]
    [SerializeField] private float verticalSpeedMulStart = 1.35f;

    [Tooltip("Speed multiplier at the end of a vertical throw.")]
    [SerializeField] private float verticalSpeedMulEnd = 0.75f;

    // ============================================================
    // Landing / Pickup Spawn
    // ============================================================

    [Header("Drop Pickup On Land")]
    [Tooltip("ItemPickup prefab to spawn at the final landing position.")]
    [SerializeField] private GameObject pickupPrefab;

    [Tooltip("If true: spawns pickupPrefab on landing (unless the projectile despawns due to special hits).")]
    [SerializeField] private bool spawnPickupOnLand = true;

    // ============================================================
    // Visual Spin
    // ============================================================

    [Header("Spin (Visual)")]
    [Tooltip("If true: rotates the projectile while moving.")]
    [SerializeField] private bool spinEnabled = true;

    [Tooltip("Random spin speed range in degrees per second.")]
    [SerializeField] private Vector2 spinSpeedRange = new Vector2(60f, 220f);

    [Tooltip("Additional random spin kick applied when a bounce starts.")]
    [SerializeField] private float bounceSpinKick = 60f;

    [Tooltip("If true: resets projectile rotation to identity on landing.")]
    [SerializeField] private bool resetRotationOnLand = false;

    // ============================================================
    // Runtime (flight settings)
    // ============================================================

    private float flightSpeed;
    private float arcHeight;

    // Ground motion (no arc)
    private Vector3 groundStart;
    private Vector3 groundTarget;
    private Vector3 groundPos;
    private float totalGroundDistance;

    private bool active;

    private Vector2 inheritedVelocity;
    private Vector2 arcPerp;

    private bool isBouncing;
    private Vector2 bounceDir;
    private float currentBounceSpeed;

    // Spin runtime (signed)
    private float spinSpeedDeg;

    // Sidewall state machine
    private bool sideDropPlanned;        // Aimed at sidewall face -> start sliding down once arriving
    private bool sideSlidingDown;        // Currently sliding down to leave the sidewall collider
    private bool sideSpiralDropPending;  // After leaving collider, perform one spiral drop segment

    /// <summary>
    /// Initializes and starts the flight.
    /// </summary>
    /// <param name="targetWorldPos">Target world position (Z is forced to 0).</param>
    /// <param name="speed">Base flight speed.</param>
    /// <param name="arc">Arc height (sinus arc). Can be 0 for no arc.</param>
    /// <param name="inherited">Velocity inherited from the player movement (applied lightly each frame).</param>
    public void Init(Vector3 targetWorldPos, float speed, float arc, Vector2 inherited)
    {
        // Resolve persistent spawner (singleton)
        if (ghostSpawner == null)
            ghostSpawner = GhostSpawner.I;

        // Cache player once (for GhostSpawningEnabled checks)
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        Vector3 start = transform.position;
        start.z = 0f;

        Vector3 desired = targetWorldPos;
        desired.z = 0f;

        flightSpeed = Mathf.Max(0.01f, speed);
        arcHeight = Mathf.Max(0f, arc);
        inheritedVelocity = inherited;

        isBouncing = false;
        bounceDir = Vector2.zero;

        sideDropPlanned = false;
        sideSlidingDown = false;
        sideSpiralDropPending = false;

        groundPos = start;
        groundStart = start;
        groundTarget = desired;

        // Random spin
        if (spinEnabled)
        {
            float s = Random.Range(spinSpeedRange.x, spinSpeedRange.y);
            spinSpeedDeg = Random.value < 0.5f ? -s : s;
        }
        else
        {
            spinSpeedDeg = 0f;
        }

        // ============================================================
        // Top barrier pre-check (prevents throwing over a side wall)
        // ============================================================
        if (wallTopBarier.value != 0)
        {
            Vector2 s = groundStart;
            Vector2 d = groundTarget;

            RaycastHit2D topHit = Physics2D.Linecast(s, d, wallTopBarier);
            if (topHit.collider != null)
            {
                // Clamp target to the barrier hit point (with a small offset)
                Vector2 hitPoint = topHit.point + topHit.normal * wallDistance;
                groundTarget = new Vector3(hitPoint.x, hitPoint.y, 0f);

                bounceDir = Vector2.zero;

                // Enter sidewall behavior after arriving
                sideDropPlanned = true;

                RecomputeArc(groundStart, groundTarget);
                active = true;
                transform.position = groundPos;
                return;
            }
        }

        // ============================================================
        // Sidewall plan (if target point overlaps the sidewall face)
        // ============================================================
        if (wallMaskSideView.value != 0)
        {
            Collider2D sideAtTarget = Physics2D.OverlapPoint(desired, wallMaskSideView);
            if (sideAtTarget != null)
                sideDropPlanned = true;
        }

        // ============================================================
        // Top-view wall pre-check (plan a bounce if a wall blocks the line)
        // ============================================================
        if (wallMaskTopView.value != 0)
        {
            Vector2 s = groundStart;
            Vector2 d = groundTarget;

            RaycastHit2D wall = Physics2D.Linecast(s, d, wallMaskTopView);
            if (wall.collider != null)
            {
                Vector2 incomingDir = (d - s);
                if (incomingDir.sqrMagnitude < 0.000001f) incomingDir = Vector2.right;
                incomingDir.Normalize();

                // Reflect direction
                bounceDir = Vector2.Reflect(incomingDir, wall.normal).normalized;

                // Clamp target to wall hit point (with a small offset)
                Vector2 wallPoint = wall.point + wall.normal * wallDistance;
                groundTarget = new Vector3(wallPoint.x, wallPoint.y, 0f);

                // Debug surface info (optional)
                var surface = wall.collider.GetComponent<MaterialHit>();
                if (surface != null)
                    Debug.Log($"Impact: {surface.type} ({wall.collider.name})");
                else
                    Debug.Log($"Impact: Wall ({wall.collider.name})");
            }
        }

        RecomputeArc(groundStart, groundTarget);
        active = true;
        transform.position = groundPos;
    }

    private void Update()
    {
        if (!active) return;

        // ============================================================
        // Sidewall sliding down (special state)
        // ============================================================
        if (sideSlidingDown)
        {
            Vector3 pos = transform.position;

            float step = Mathf.Max(sideSlideSpeed * Time.deltaTime, sideSlideMinStep);
            pos += Vector3.down * step;
            pos.z = 0f;
            transform.position = pos;

            if (spinEnabled && Mathf.Abs(spinSpeedDeg) > 0.001f)
                transform.Rotate(0f, 0f, spinSpeedDeg * Time.deltaTime);

            // Leave slide state once no longer inside sidewall collider
            Collider2D stillInside = Physics2D.OverlapPoint(pos, wallMaskSideView);
            if (stillInside == null)
            {
                sideSlidingDown = false;
                sideSpiralDropPending = true;

                // Reset ground motion state to current position
                groundPos = pos;
                groundStart = pos;
                groundTarget = pos;
            }

            return;
        }

        // ============================================================
        // Normal flight / bounce segments
        // ============================================================
        float baseSpeed = isBouncing ? currentBounceSpeed : flightSpeed;

        float tCurrent = totalGroundDistance <= 0.0001f
            ? 1f
            : Vector3.Distance(groundStart, groundPos) / totalGroundDistance;
        tCurrent = Mathf.Clamp01(tCurrent);

        // Vertical throw "ease-out" multiplier
        float speedMul = 1f;
        if (!isBouncing && easeOutOnVerticalThrows && arcPerp == Vector2.zero)
            speedMul = Mathf.Lerp(verticalSpeedMulStart, verticalSpeedMulEnd, tCurrent);

        float stepFlight = baseSpeed * speedMul * Time.deltaTime;

        Vector3 nextGround = Vector3.MoveTowards(groundPos, groundTarget, stepFlight);
        nextGround += (Vector3)(inheritedVelocity * 0.05f * Time.deltaTime);
        nextGround.z = 0f;

        float t = totalGroundDistance <= 0.0001f
            ? 1f
            : Vector3.Distance(groundStart, nextGround) / totalGroundDistance;
        t = Mathf.Clamp01(t);

        // Sin arc
        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        Vector3 arcOffset = (Vector3)(arcPerp * arc);

        transform.position = nextGround + arcOffset;
        groundPos = nextGround;

        // Spin
        if (spinEnabled && Mathf.Abs(spinSpeedDeg) > 0.001f)
            transform.Rotate(0f, 0f, spinSpeedDeg * Time.deltaTime);

        // Bounce drag
        if (isBouncing)
            currentBounceSpeed = Mathf.Lerp(currentBounceSpeed, 0, wallBounceDrag * Time.deltaTime);

        // Arrive checks / state transitions
        if (Vector2.Distance(groundPos, groundTarget) <= 0.05f)
        {
            // Sidewall entry: play noise and start sliding down
            if (!isBouncing && bounceDir == Vector2.zero && sideDropPlanned)
            {
                PlayNoise("Wall", (Vector2)groundPos, firstHit: true);

                sideDropPlanned = false;
                sideSlidingDown = true;
                return;
            }

            // After leaving sidewall collider: start one spiral drop segment
            if (sideSpiralDropPending && !isBouncing)
            {
                StartSideSpiralDrop();
                return;
            }

            // Bounce segment start
            if (!isBouncing && bounceDir != Vector2.zero)
            {
                StartBounce();
                return;
            }

            // Final landing
            Land();
        }
    }

    /// <summary>
    /// Starts a downward spiral drop segment after the projectile leaves the sidewall collider.
    /// </summary>
    private void StartSideSpiralDrop()
    {
        sideSpiralDropPending = false;

        isBouncing = true;
        currentBounceSpeed = wallBounceSpeed;

        // Optional spin kick
        if (spinEnabled && bounceSpinKick > 0f)
        {
            float kick = Random.Range(-bounceSpinKick, bounceSpinKick);
            spinSpeedDeg += kick;
        }

        float dist = Random.Range(sideSpiralDropDistanceMin, sideSpiralDropDistanceMax);
        Vector2 wanted = (Vector2)groundPos + Vector2.down * dist;

        groundStart = groundPos;
        groundTarget = new Vector3(wanted.x, wanted.y, 0f);

        // Side drop uses a simple upward arc (visual)
        arcPerp = Vector2.up;
        totalGroundDistance = Vector3.Distance(groundStart, groundTarget);

        bounceDir = Vector2.zero;
    }

    /// <summary>
    /// Starts a bounce segment in the precomputed bounce direction.
    /// </summary>
    private void StartBounce()
    {
        // Bounce noise (Wall)
        PlayNoise("Wall", (Vector2)groundPos, firstHit: true);

        isBouncing = true;
        currentBounceSpeed = wallBounceSpeed;

        // Optional spin kick
        if (spinEnabled && bounceSpinKick > 0f)
        {
            float kick = Random.Range(-bounceSpinKick, bounceSpinKick);
            spinSpeedDeg += kick;
        }

        float bounceDistance = Random.Range(wallBounceDistanceMin, wallBounceDistanceMax);
        Vector2 wanted = (Vector2)groundPos + bounceDir * bounceDistance;

        // Prevent bouncing into a wall again (top view)
        if (wallMaskTopView.value != 0)
        {
            RaycastHit2D wall = Physics2D.Linecast(groundPos, wanted, wallMaskTopView);
            if (wall.collider != null)
            {
                Vector2 safe = wall.point + wall.normal * wallDistance;
                wanted = safe;
            }
        }

        groundStart = groundPos;
        groundTarget = new Vector3(wanted.x, wanted.y, 0f);

        RecomputeArc(groundStart, groundTarget);
        bounceDir = Vector2.zero;
    }

    /// <summary>
    /// Finalizes the projectile landing:
    /// - clamps away from walls if needed
    /// - detects surface MaterialHit / BreakableWindow
    /// - plays noise on surface hit
    /// - spawns a pickup item (optional)
    /// - destroys the projectile
    /// </summary>
    private void Land()
    {
        active = false;
        Vector3 finalGround = groundTarget;

        // Clamp away from top-view walls to avoid overlap
        if (wallMaskTopView.value != 0)
        {
            Collider2D wallCol = Physics2D.OverlapCircle(finalGround, 0.02f, wallMaskTopView);
            if (wallCol != null)
            {
                Vector2 closest = wallCol.ClosestPoint(finalGround);
                Vector2 pushDir = (Vector2)finalGround - closest;

                if (pushDir.sqrMagnitude < 0.000001f)
                    pushDir = (Vector2)(finalGround - wallCol.transform.position);
                if (pushDir.sqrMagnitude < 0.000001f)
                    pushDir = Vector2.up;

                Vector2 safePos = closest + pushDir.normalized * (wallDistance + 0.03f);
                finalGround = new Vector3(safePos.x, safePos.y, 0f);
            }
        }

        transform.position = finalGround;

        if (resetRotationOnLand)
            transform.rotation = Quaternion.identity;

        Vector2 impactPoint = finalGround;

        // Detect surface at landing
        Collider2D hit = Physics2D.OverlapCircle(impactPoint, impactRadius, surfaceMask);

        bool shouldDespawn = false;

        if (hit != null)
        {
            // Breakable windows: break and despawn the projectile
            var window = hit.GetComponentInParent<BreakableWindow>();
            if (window != null)
            {
                window.OnHit();
                shouldDespawn = true;
            }

            // Surface sound: only if not a window hit
            var surface = hit.GetComponent<MaterialHit>();
            if (surface != null)
            {
                Debug.Log($"Impact: {surface.type} ({hit.gameObject.name})");

                if (window == null)
                {
                    PlayNoise(surface.type.ToString(), impactPoint, firstHit: true);
                }
            }
            else
            {
                Debug.Log($"Impact: (no SurfaceMaterial) Tag={hit.tag} ({hit.gameObject.name})");
            }
        }
        else
        {
            Debug.Log("Impact: (nothing) -> not on Surface layer / outside mask");
        }

        // Spawn pickup on land (unless a special hit requested despawn)
        if (!shouldDespawn && spawnPickupOnLand && pickupPrefab != null)
        {
            GameObject pickup = Instantiate(pickupPrefab, finalGround, Quaternion.identity);

            // Any pickup spawned from a thrown projectile should not be usable again.
            ItemPickup ip = pickup.GetComponent<ItemPickup>();
            if (ip != null)
                ip.SetUsable(false);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Recomputes arc perpendicular direction and total ground distance for the current segment.
    /// </summary>
    private void RecomputeArc(Vector3 fromGround, Vector3 toGround)
    {
        groundStart = fromGround;
        groundTarget = toGround;

        Vector2 dir = ((Vector2)groundTarget - (Vector2)groundStart);
        if (dir.sqrMagnitude < 0.00001f) dir = Vector2.right;
        dir.Normalize();

        // If the throw is mostly horizontal, use an upward arc; otherwise no arc (vertical easing handles perception).
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            arcPerp = Vector2.up;
        else
            arcPerp = Vector2.zero;

        totalGroundDistance = Vector3.Distance(groundStart, groundTarget);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(groundTarget, impactRadius);
    }

    // ============================================================
    // Noise helper
    // - Plays SFX via FX_SoundSystem
    // - Spawns a static light at the noise position (optional)
    // - Retargets/spawns the ghost at the noise position (optional)
    // ============================================================

    /// <summary>
    /// Plays a "noise event" at a world position:
    /// 1) Plays an SFX by key using FX_SoundSystem.
    /// 2) Optionally spawns a permanent light at the noise point.
    /// 3) Optionally retargets/spawns the ghost at the noise point (gated by PlayerController.GhostSpawningEnabled).
    /// </summary>
    /// <param name="soundKey">SFX tag/key passed to FX_SoundSystem (e.g., "Wall", "Grass", "Wood").</param>
    /// <param name="worldPos">World position where the noise happened.</param>
    /// <param name="firstHit">If true, uses the "firstHit" clip for that tag; otherwise uses "repeatHit".</param>
    private void PlayNoise(string soundKey, Vector2 worldPos, bool firstHit)
    {
        // 1) Sound
        FX_SoundSystem.I?.PlayHit(soundKey, firstHit);

        // 2) Permanent light on noise point (optional)
        if (spawnStaticLightOnNoise && noiseLightStaticPrefab != null)
        {
            Vector3 p = new Vector3(worldPos.x, worldPos.y, 0f);
            Instantiate(noiseLightStaticPrefab, p, Quaternion.identity);
        }

        // 3) Ghost retarget/spawn (optional + gated)
        if (!triggerGhostOnNoise) return;

        // Re-resolve player if needed (scene changes)
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        // Player can globally disable ghost reactions
        if (playerController != null && !playerController.GhostSpawningEnabled)
            return;

        GhostSpawner spawner = ghostSpawner != null ? ghostSpawner : GhostSpawner.I;
        if (spawner != null)
            spawner.SpawnGhostTo(worldPos);
    }
}