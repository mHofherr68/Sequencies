/*using UnityEngine;          // WallTopBarier blocks over-throw AND triggers sidewall drop

public class ThrowableFlight : MonoBehaviour
{
    [Header("Impact")]
    [SerializeField] private float impactRadius = 0.25f;

    [Header("Detection")]
    [SerializeField] private LayerMask surfaceMask = ~0;

    // TM_Wall_01 (TopView) - original
    [SerializeField] private LayerMask wallMaskTopView = 0;

    // TM_Wall_02 (SideView) - face (tilemap)
    [SerializeField] private LayerMask wallMaskSideView = 0;

    // TopCollider on TM_Wall_02 (child) to prevent throwing over from above
    [SerializeField] private LayerMask wallTopBarier = 0; // set to WallTopBarier

    [Header("Wall Bounce")]
    [SerializeField] private float wallDistance = 0.05f;
    [SerializeField] private float wallBounceDistanceMin = 0.25f;
    [SerializeField] private float wallBounceDistanceMax = 0.55f;
    [SerializeField] private float wallBounceSpeed = 8f;
    [SerializeField] private float wallBounceDrag = 4f;

    [Header("SideWall Slide (TM_Wall_02)")]
    [Tooltip("Geschwindigkeit, mit der das Item die Sidewall nach unten verlässt.")]
    [SerializeField] private float sideSlideSpeed = 7.5f;

    [Tooltip("Minimaler Schritt nach unten pro Frame, damit der Collider sicher verlassen wird.")]
    [SerializeField] private float sideSlideMinStep = 0.02f;

    [Header("SideWall Spiral Drop (after leaving collider)")]
    [Tooltip("Wie weit das Item nach Verlassen der Sidewall noch spiralförmig nach unten fliegt (Min).")]
    [SerializeField] private float sideSpiralDropDistanceMin = 0.45f;

    [Tooltip("Wie weit das Item nach Verlassen der Sidewall noch spiralförmig nach unten fliegt (Max).")]
    [SerializeField] private float sideSpiralDropDistanceMax = 1.10f;

    [Header("Perceived Speed (Up/Down Throws)")]
    [SerializeField] private bool easeOutOnVerticalThrows = true;
    [SerializeField] private float verticalSpeedMulStart = 1.35f;
    [SerializeField] private float verticalSpeedMulEnd = 0.75f;

    [Header("Drop Pickup On Land")]
    [SerializeField] private GameObject pickupPrefab;
    [SerializeField] private bool spawnPickupOnLand = true;

    [Header("Spin (Visual)")]
    [SerializeField] private bool spinEnabled = true;
    [SerializeField] private Vector2 spinSpeedRange = new Vector2(60f, 220f);
    [SerializeField] private float bounceSpinKick = 60f;
    [SerializeField] private bool resetRotationOnLand = false;

    // Runtime only (settings)
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

    // Spin runtime
    private float spinSpeedDeg; // signed

    // Sidewall state machine
    private bool sideDropPlanned;        // we aimed at sidewall face -> after arrival start sliding down
    private bool sideSlidingDown;        // currently sliding down to leave the sidewall collider
    private bool sideSpiralDropPending;  // after leaving collider, start spiral drop segment once

    public void Init(Vector3 targetWorldPos, float speed, float arc, Vector2 inherited)
    {
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

        if (spinEnabled)
        {
            float s = Random.Range(spinSpeedRange.x, spinSpeedRange.y);
            spinSpeedDeg = Random.value < 0.5f ? -s : s;
        }
        else spinSpeedDeg = 0f;

        // ============================================================
        // TopCollider pre-check (WallTopBarier)
        // NEW BEHAVIOR:
        // - Block the throw (clamp to hit point, play sound)
        // - THEN behave like sidewall: slide down to leave wallMaskSide, spiral drop, land.
        // ============================================================
        if (wallTopBarier.value != 0)
        {
            Vector2 s = groundStart;
            Vector2 d = groundTarget;

            RaycastHit2D topHit = Physics2D.Linecast(s, d, wallTopBarier);
            if (topHit.collider != null)
            {
                // clamp to the top barrier hit point (so it cannot go through)
                Vector2 hitPoint = topHit.point + topHit.normal * wallDistance;
                groundTarget = new Vector3(hitPoint.x, hitPoint.y, 0f);

                // IMPORTANT: do NOT reflect/bounce here, otherwise it can end up sitting on the wall.
                bounceDir = Vector2.zero;

                // Once we arrive at this clamped point, immediately start sliding down (sidewall logic)
                sideDropPlanned = true;
                RecomputeArc(groundStart, groundTarget);
                active = true;
                transform.position = groundPos;
                return;
            }
        }

        // ============================================================
        // Sidewall plan (TM_Wall_02): If target point is on sidewall,
        // we allow reaching it, then slide out, then do spiral drop.
        // ============================================================
        if (wallMaskSideView.value != 0)
        {
            Collider2D sideAtTarget = Physics2D.OverlapPoint(desired, wallMaskSideView);
            if (sideAtTarget != null)
            {
                sideDropPlanned = true;
                // We do NOT clamp to edge; we WANT to hit the wall face.
            }
        }

        // ============================================================
        // ORIGINAL TM_Wall_01 pre-check (UNCHANGED)
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

                bounceDir = Vector2.Reflect(incomingDir, wall.normal).normalized;

                Vector2 wallPoint = wall.point + wall.normal * wallDistance;
                groundTarget = new Vector3(wallPoint.x, wallPoint.y, 0f);

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
        // Sidewall sliding down: leave TM_Wall_02 collider, then start spiral drop.
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

            Collider2D stillInside = Physics2D.OverlapPoint(pos, wallMaskSideView);
            if (stillInside == null)
            {
                sideSlidingDown = false;
                sideSpiralDropPending = true;

                groundPos = pos;
                groundStart = pos;
                groundTarget = pos;
            }

            return;
        }

        // ============================================================
        // NORMAL FLIGHT (original) including spiral drop segment
        // ============================================================
        float baseSpeed = isBouncing ? currentBounceSpeed : flightSpeed;

        float tCurrent = totalGroundDistance <= 0.0001f ? 1f : Vector3.Distance(groundStart, groundPos) / totalGroundDistance;
        tCurrent = Mathf.Clamp01(tCurrent);

        float speedMul = 1f;
        if (!isBouncing && easeOutOnVerticalThrows && arcPerp == Vector2.zero)
            speedMul = Mathf.Lerp(verticalSpeedMulStart, verticalSpeedMulEnd, tCurrent);

        float stepFlight = baseSpeed * speedMul * Time.deltaTime;

        Vector3 nextGround = Vector3.MoveTowards(groundPos, groundTarget, stepFlight);
        nextGround += (Vector3)(inheritedVelocity * 0.05f * Time.deltaTime);
        nextGround.z = 0f;

        float t = totalGroundDistance <= 0.0001f ? 1f : Vector3.Distance(groundStart, nextGround) / totalGroundDistance;
        t = Mathf.Clamp01(t);

        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        Vector3 arcOffset = (Vector3)(arcPerp * arc);

        transform.position = nextGround + arcOffset;
        groundPos = nextGround;

        if (spinEnabled && Mathf.Abs(spinSpeedDeg) > 0.001f)
            transform.Rotate(0f, 0f, spinSpeedDeg * Time.deltaTime);

        if (isBouncing)
            currentBounceSpeed = Mathf.Lerp(currentBounceSpeed, 0, wallBounceDrag * Time.deltaTime);

        if (Vector2.Distance(groundPos, groundTarget) <= 0.05f)
        {
            // 1) Arrived on sidewall face/top barrier clamp point -> start sliding out of wallMaskSide
            if (!isBouncing && bounceDir == Vector2.zero && sideDropPlanned)
            {
                // play wall hit only once (top barrier already played it, but harmless if duplicated)
                FX_SoundSystem.I?.PlayHit("Wall", firstHit: true);

                sideDropPlanned = false;
                sideSlidingDown = true;
                return;
            }

            // 2) After leaving collider -> start spiral drop segment once
            if (sideSpiralDropPending && !isBouncing)
            {
                StartSideSpiralDrop();
                return;
            }

            // 3) Original bounce for TM_Wall_01
            if (!isBouncing && bounceDir != Vector2.zero)
            {
                StartBounce();
                return;
            }

            Land();
        }
    }

    private void StartSideSpiralDrop()
    {
        sideSpiralDropPending = false;

        isBouncing = true;
        currentBounceSpeed = wallBounceSpeed;

        if (spinEnabled && bounceSpinKick > 0f)
        {
            float kick = Random.Range(-bounceSpinKick, bounceSpinKick);
            spinSpeedDeg += kick;
        }

        float dist = Random.Range(sideSpiralDropDistanceMin, sideSpiralDropDistanceMax);
        Vector2 wanted = (Vector2)groundPos + Vector2.down * dist;

        groundStart = groundPos;
        groundTarget = new Vector3(wanted.x, wanted.y, 0f);

        // keep your current spiral look
        arcPerp = Vector2.up;
        totalGroundDistance = Vector3.Distance(groundStart, groundTarget);

        bounceDir = Vector2.zero;
    }

    private void StartBounce()
    {
        FX_SoundSystem.I?.PlayHit("Wall", firstHit: true);
        isBouncing = true;
        currentBounceSpeed = wallBounceSpeed;

        if (spinEnabled && bounceSpinKick > 0f)
        {
            float kick = Random.Range(-bounceSpinKick, bounceSpinKick);
            spinSpeedDeg += kick;
        }

        float bounceDistance = Random.Range(wallBounceDistanceMin, wallBounceDistanceMax);
        Vector2 wanted = (Vector2)groundPos + bounceDir * bounceDistance;

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

    private void Land()
    {
        active = false;

        Vector3 finalGround = groundTarget;

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
        Collider2D hit = Physics2D.OverlapCircle(impactPoint, impactRadius, surfaceMask);

        bool shouldDespawn = false;

        if (hit != null)
        {
            var window = hit.GetComponentInParent<BreakableWindow>();
            if (window != null)
            {
                window.OnHit();
                shouldDespawn = true;
            }

            var surface = hit.GetComponent<MaterialHit>();
            if (surface != null)
                Debug.Log($"Impact: {surface.type} ({hit.gameObject.name})");

            if (window == null && surface != null)
                FX_SoundSystem.I?.PlayHit(surface.type, firstHit: true);

            if (surface == null)
                Debug.Log($"Impact: (no SurfaceMaterial) Tag={hit.tag} ({hit.gameObject.name})");
        }
        else
        {
            Debug.Log("Impact: (nothing) -> not on Surface layer / outside mask");
        }

        if (!shouldDespawn && spawnPickupOnLand && pickupPrefab != null)
            Instantiate(pickupPrefab, finalGround, Quaternion.identity);

        Destroy(gameObject);
    }

    private void RecomputeArc(Vector3 fromGround, Vector3 toGround)
    {
        groundStart = fromGround;
        groundTarget = toGround;

        Vector2 dir = ((Vector2)groundTarget - (Vector2)groundStart);
        if (dir.sqrMagnitude < 0.00001f) dir = Vector2.right;
        dir.Normalize();

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
}*/
using UnityEngine;          // WallTopBarier blocks over-throw AND triggers sidewall drop

public class ThrowableFlight : MonoBehaviour
{
    [Header("Impact")]
    [SerializeField] private float impactRadius = 0.25f;

    [Header("Detection")]
    [SerializeField] private LayerMask surfaceMask = ~0;

    // TM_Wall_01 (TopView) - original
    [SerializeField] private LayerMask wallMaskTopView = 0;

    // TM_Wall_02 (SideView) - face (tilemap)
    [SerializeField] private LayerMask wallMaskSideView = 0;

    // TopCollider on TM_Wall_02 (child) to prevent throwing over from above
    [SerializeField] private LayerMask wallTopBarier = 0; // set to WallTopBarier

    [Header("Wall Bounce")]
    [SerializeField] private float wallDistance = 0.05f;
    [SerializeField] private float wallBounceDistanceMin = 0.25f;
    [SerializeField] private float wallBounceDistanceMax = 0.55f;
    [SerializeField] private float wallBounceSpeed = 8f;
    [SerializeField] private float wallBounceDrag = 4f;

    [Header("SideWall Slide (TM_Wall_02)")]
    [Tooltip("Geschwindigkeit, mit der das Item die Sidewall nach unten verlässt.")]
    [SerializeField] private float sideSlideSpeed = 7.5f;

    [Tooltip("Minimaler Schritt nach unten pro Frame, damit der Collider sicher verlassen wird.")]
    [SerializeField] private float sideSlideMinStep = 0.02f;

    [Header("SideWall Spiral Drop (after leaving collider)")]
    [Tooltip("Wie weit das Item nach Verlassen der Sidewall noch spiralförmig nach unten fliegt (Min).")]
    [SerializeField] private float sideSpiralDropDistanceMin = 0.45f;

    [Tooltip("Wie weit das Item nach Verlassen der Sidewall noch spiralförmig nach unten fliegt (Max).")]
    [SerializeField] private float sideSpiralDropDistanceMax = 1.10f;

    [Header("Perceived Speed (Up/Down Throws)")]
    [SerializeField] private bool easeOutOnVerticalThrows = true;
    [SerializeField] private float verticalSpeedMulStart = 1.35f;
    [SerializeField] private float verticalSpeedMulEnd = 0.75f;

    [Header("Drop Pickup On Land")]
    [SerializeField] private GameObject pickupPrefab;
    [SerializeField] private bool spawnPickupOnLand = true;

    [Header("Spin (Visual)")]
    [SerializeField] private bool spinEnabled = true;
    [SerializeField] private Vector2 spinSpeedRange = new Vector2(60f, 220f);
    [SerializeField] private float bounceSpinKick = 60f;
    [SerializeField] private bool resetRotationOnLand = false;

    // Runtime only (settings)
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

    // Spin runtime
    private float spinSpeedDeg; // signed

    // Sidewall state machine
    private bool sideDropPlanned;        // we aimed at sidewall face -> after arrival start sliding down
    private bool sideSlidingDown;        // currently sliding down to leave the sidewall collider
    private bool sideSpiralDropPending;  // after leaving collider, start spiral drop segment once

    public void Init(Vector3 targetWorldPos, float speed, float arc, Vector2 inherited)
    {
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

        if (spinEnabled)
        {
            float s = Random.Range(spinSpeedRange.x, spinSpeedRange.y);
            spinSpeedDeg = Random.value < 0.5f ? -s : s;
        }
        else spinSpeedDeg = 0f;

        // ============================================================
        // TopCollider pre-check (WallTopBarier)
        // ============================================================
        if (wallTopBarier.value != 0)
        {
            Vector2 s = groundStart;
            Vector2 d = groundTarget;

            RaycastHit2D topHit = Physics2D.Linecast(s, d, wallTopBarier);
            if (topHit.collider != null)
            {
                Vector2 hitPoint = topHit.point + topHit.normal * wallDistance;
                groundTarget = new Vector3(hitPoint.x, hitPoint.y, 0f);

                bounceDir = Vector2.zero;

                sideDropPlanned = true;
                RecomputeArc(groundStart, groundTarget);
                active = true;
                transform.position = groundPos;
                return;
            }
        }

        // ============================================================
        // Sidewall plan (TM_Wall_02)
        // ============================================================
        if (wallMaskSideView.value != 0)
        {
            Collider2D sideAtTarget = Physics2D.OverlapPoint(desired, wallMaskSideView);
            if (sideAtTarget != null)
                sideDropPlanned = true;
        }

        // ============================================================
        // ORIGINAL TM_Wall_01 pre-check (UNCHANGED)
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

                bounceDir = Vector2.Reflect(incomingDir, wall.normal).normalized;

                Vector2 wallPoint = wall.point + wall.normal * wallDistance;
                groundTarget = new Vector3(wallPoint.x, wallPoint.y, 0f);

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
        // Sidewall sliding down
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

            Collider2D stillInside = Physics2D.OverlapPoint(pos, wallMaskSideView);
            if (stillInside == null)
            {
                sideSlidingDown = false;
                sideSpiralDropPending = true;

                groundPos = pos;
                groundStart = pos;
                groundTarget = pos;
            }

            return;
        }

        // ============================================================
        // NORMAL FLIGHT
        // ============================================================
        float baseSpeed = isBouncing ? currentBounceSpeed : flightSpeed;

        float tCurrent = totalGroundDistance <= 0.0001f ? 1f : Vector3.Distance(groundStart, groundPos) / totalGroundDistance;
        tCurrent = Mathf.Clamp01(tCurrent);

        float speedMul = 1f;
        if (!isBouncing && easeOutOnVerticalThrows && arcPerp == Vector2.zero)
            speedMul = Mathf.Lerp(verticalSpeedMulStart, verticalSpeedMulEnd, tCurrent);

        float stepFlight = baseSpeed * speedMul * Time.deltaTime;

        Vector3 nextGround = Vector3.MoveTowards(groundPos, groundTarget, stepFlight);
        nextGround += (Vector3)(inheritedVelocity * 0.05f * Time.deltaTime);
        nextGround.z = 0f;

        float t = totalGroundDistance <= 0.0001f ? 1f : Vector3.Distance(groundStart, nextGround) / totalGroundDistance;
        t = Mathf.Clamp01(t);

        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        Vector3 arcOffset = (Vector3)(arcPerp * arc);

        transform.position = nextGround + arcOffset;
        groundPos = nextGround;

        if (spinEnabled && Mathf.Abs(spinSpeedDeg) > 0.001f)
            transform.Rotate(0f, 0f, spinSpeedDeg * Time.deltaTime);

        if (isBouncing)
            currentBounceSpeed = Mathf.Lerp(currentBounceSpeed, 0, wallBounceDrag * Time.deltaTime);

        if (Vector2.Distance(groundPos, groundTarget) <= 0.05f)
        {
            if (!isBouncing && bounceDir == Vector2.zero && sideDropPlanned)
            {
                FX_SoundSystem.I?.PlayHit("Wall", firstHit: true);

                sideDropPlanned = false;
                sideSlidingDown = true;
                return;
            }

            if (sideSpiralDropPending && !isBouncing)
            {
                StartSideSpiralDrop();
                return;
            }

            if (!isBouncing && bounceDir != Vector2.zero)
            {
                StartBounce();
                return;
            }

            Land();
        }
    }

    private void StartSideSpiralDrop()
    {
        sideSpiralDropPending = false;

        isBouncing = true;
        currentBounceSpeed = wallBounceSpeed;

        if (spinEnabled && bounceSpinKick > 0f)
        {
            float kick = Random.Range(-bounceSpinKick, bounceSpinKick);
            spinSpeedDeg += kick;
        }

        float dist = Random.Range(sideSpiralDropDistanceMin, sideSpiralDropDistanceMax);
        Vector2 wanted = (Vector2)groundPos + Vector2.down * dist;

        groundStart = groundPos;
        groundTarget = new Vector3(wanted.x, wanted.y, 0f);

        arcPerp = Vector2.up;
        totalGroundDistance = Vector3.Distance(groundStart, groundTarget);

        bounceDir = Vector2.zero;
    }

    private void StartBounce()
    {
        FX_SoundSystem.I?.PlayHit("Wall", firstHit: true);
        isBouncing = true;
        currentBounceSpeed = wallBounceSpeed;

        if (spinEnabled && bounceSpinKick > 0f)
        {
            float kick = Random.Range(-bounceSpinKick, bounceSpinKick);
            spinSpeedDeg += kick;
        }

        float bounceDistance = Random.Range(wallBounceDistanceMin, wallBounceDistanceMax);
        Vector2 wanted = (Vector2)groundPos + bounceDir * bounceDistance;

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

    private void Land()
    {
        active = false;

        Vector3 finalGround = groundTarget;

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
        Collider2D hit = Physics2D.OverlapCircle(impactPoint, impactRadius, surfaceMask);

        bool shouldDespawn = false;

        if (hit != null)
        {
            var window = hit.GetComponentInParent<BreakableWindow>();
            if (window != null)
            {
                window.OnHit();
                shouldDespawn = true;
            }

            var surface = hit.GetComponent<MaterialHit>();
            if (surface != null)
                Debug.Log($"Impact: {surface.type} ({hit.gameObject.name})");

            if (window == null && surface != null)
                FX_SoundSystem.I?.PlayHit(surface.type, firstHit: true);

            if (surface == null)
                Debug.Log($"Impact: (no SurfaceMaterial) Tag={hit.tag} ({hit.gameObject.name})");
        }
        else
        {
            Debug.Log("Impact: (nothing) -> not on Surface layer / outside mask");
        }

        if (!shouldDespawn && spawnPickupOnLand && pickupPrefab != null)
        {
            GameObject pickup = Instantiate(pickupPrefab, finalGround, Quaternion.identity);

            // KEY FIX: Anything that spawns from a thrown projectile should NOT be usable again.
            ItemPickup ip = pickup.GetComponent<ItemPickup>();
            if (ip != null)
                ip.SetUsable(false);
        }

        Destroy(gameObject);
    }

    private void RecomputeArc(Vector3 fromGround, Vector3 toGround)
    {
        groundStart = fromGround;
        groundTarget = toGround;

        Vector2 dir = ((Vector2)groundTarget - (Vector2)groundStart);
        if (dir.sqrMagnitude < 0.00001f) dir = Vector2.right;
        dir.Normalize();

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
}
