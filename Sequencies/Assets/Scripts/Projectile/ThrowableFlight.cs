/*using UnityEngine;                // Absolut OK. Kleiner Wall Bug

public class ThrowableFlight : MonoBehaviour
{
    [Header("Impact")]
    [SerializeField] private float impactRadius = 0.25f;

    [Header("Detection")]
    [SerializeField] private LayerMask surfaceMask = ~0;
    [SerializeField] private LayerMask wallMask = 0;

    [Header("Wall Bounce")]
    [SerializeField] private float wallDistance = 0.05f;
    [SerializeField] private float wallBounceDistanceMin = 0.25f;
    [SerializeField] private float wallBounceDistanceMax = 0.55f;
    [SerializeField] private float wallBounceSpeed = 8f;
    [SerializeField] private float wallBounceDrag = 4f;

    [Header("Perceived Speed (Up/Down Throws)")]
    [Tooltip("Wenn true: bei vertikalen Würfen (oben/unten) startet der Flug schneller und wird zum Ende hin langsamer.")]
    [SerializeField] private bool easeOutOnVerticalThrows = true;

    [Tooltip("Multiplikator am Start (t=0). >1 = schneller am Anfang.")]
    [SerializeField] private float verticalSpeedMulStart = 1.35f;

    [Tooltip("Multiplikator am Ende (t=1). <1 = langsamer am Ende.")]
    [SerializeField] private float verticalSpeedMulEnd = 0.75f;

    [Header("Drop Pickup On Land")]
    [Tooltip("Wenn gesetzt, spawnt der geworfene Gegenstand beim Landen ein Pickup (z.B. StonePickup).")]
    [SerializeField] private GameObject pickupPrefab;

    [Tooltip("Wenn true: Pickup wird beim normalen Landen gespawnt.")]
    [SerializeField] private bool spawnPickupOnLand = true;

    // Runtime only
    private float flightSpeed;
    private float arcHeight;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float totalDistance;
    private bool active;

    private Vector2 inheritedVelocity;
    private Vector2 arcPerp;

    private bool isBouncing;
    private Vector2 bounceDir;
    private float currentBounceSpeed;

    public void Init(Vector3 targetWorldPos, float speed, float arc, Vector2 inherited)
    {
        startPos = transform.position;
        startPos.z = 0f;

        targetPos = targetWorldPos;
        targetPos.z = 0f;

        flightSpeed = Mathf.Max(0.01f, speed);
        arcHeight = Mathf.Max(0f, arc);
        inheritedVelocity = inherited;

        isBouncing = false;
        bounceDir = Vector2.zero;

        if (wallMask.value != 0)
        {
            Vector2 start = startPos;
            Vector2 desired = targetPos;

            RaycastHit2D wall = Physics2D.Linecast(start, desired, wallMask);
            if (wall.collider != null)
            {
                bounceDir = (start - wall.point).normalized;

                Vector2 wallPoint = wall.point + bounceDir * wallDistance;
                targetPos = new Vector3(wallPoint.x, wallPoint.y, 0f);

                var surface = wall.collider.GetComponent<MaterialHit>();

                if (surface != null)
                    Debug.Log($"Impact: {surface.type} ({wall.collider.name})");
                else
                    Debug.Log($"Impact: Wall ({wall.collider.name})");
            }
        }

        RecomputeArc(startPos, targetPos);
        active = true;
    }

    private void Update()
    {
        if (!active) return;

        float baseSpeed = isBouncing ? currentBounceSpeed : flightSpeed;

        // Fortschritt basierend auf aktueller Position (vor dem Schritt)
        float tCurrent = totalDistance <= 0.0001f ? 1f : Vector3.Distance(startPos, transform.position) / totalDistance;
        tCurrent = Mathf.Clamp01(tCurrent);

        // Vertikalwürfe (arcPerp == 0) starten schneller und werden zum Ende langsamer
        float speedMul = 1f;
        if (!isBouncing && easeOutOnVerticalThrows && arcPerp == Vector2.zero)
            speedMul = Mathf.Lerp(verticalSpeedMulStart, verticalSpeedMulEnd, tCurrent);

        float step = baseSpeed * speedMul * Time.deltaTime;

        Vector3 current = transform.position;
        Vector3 next = Vector3.MoveTowards(current, targetPos, step);

        float t = totalDistance <= 0.0001f ? 1f : Vector3.Distance(startPos, next) / totalDistance;
        t = Mathf.Clamp01(t);

        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        next += (Vector3)(arcPerp * arc);
        next += (Vector3)(inheritedVelocity * 0.05f * Time.deltaTime);

        next.z = 0f;
        transform.position = next;

        if (isBouncing)
        {
            currentBounceSpeed = Mathf.Lerp(currentBounceSpeed, 0, wallBounceDrag * Time.deltaTime);
        }

        if (Vector2.Distance(transform.position, targetPos) <= 0.05f)
        {
            if (!isBouncing && bounceDir != Vector2.zero)
            {
                StartBounce();
                return;
            }

            Land();
        }
    }

    private void StartBounce()
    {
        FX_SoundSystem.I?.PlayHit("Wall", firstHit: true);
        isBouncing = true;
        currentBounceSpeed = wallBounceSpeed;

        float bounceDistance = Random.Range(wallBounceDistanceMin, wallBounceDistanceMax);

        Vector2 bounceTarget = (Vector2)transform.position + bounceDir * bounceDistance;
        targetPos = new Vector3(bounceTarget.x, bounceTarget.y, 0f);

        RecomputeArc(transform.position, targetPos);
        bounceDir = Vector2.zero;
    }

    private void Land()
    {
        active = false;
        transform.position = targetPos;

        Vector2 impactPoint = targetPos;
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

        // NEU: Pickup droppen, wenn normal gelandet (nicht z.B. Fenster zerstört)
        if (!shouldDespawn && spawnPickupOnLand && pickupPrefab != null)
        {
            Instantiate(pickupPrefab, targetPos, Quaternion.identity);
        }

        // Projectile entfernen
        Destroy(gameObject);
    }

    private void RecomputeArc(Vector3 from, Vector3 to)
    {
        startPos = from;
        targetPos = to;

        Vector2 dir = ((Vector2)targetPos - (Vector2)startPos);
        if (dir.sqrMagnitude < 0.00001f) dir = Vector2.right;
        dir.Normalize();

        // Links/Rechts: Arc nach oben. Oben/Unten: gerade.
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            arcPerp = Vector2.up;
        else
            arcPerp = Vector2.zero;

        totalDistance = Vector3.Distance(startPos, targetPos);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(targetPos, impactRadius);
    }
}*/




using UnityEngine;

public class ThrowableFlight : MonoBehaviour
{
    [Header("Impact")]
    [SerializeField] private float impactRadius = 0.25f;

    [Header("Detection")]
    [SerializeField] private LayerMask surfaceMask = ~0;
    [SerializeField] private LayerMask wallMask = 0;

    [Header("Wall Bounce")]
    [SerializeField] private float wallDistance = 0.05f;
    [SerializeField] private float wallBounceDistanceMin = 0.25f;
    [SerializeField] private float wallBounceDistanceMax = 0.55f;
    [SerializeField] private float wallBounceSpeed = 8f;
    [SerializeField] private float wallBounceDrag = 4f;

    [Header("Perceived Speed (Up/Down Throws)")]
    [SerializeField] private bool easeOutOnVerticalThrows = true;
    [SerializeField] private float verticalSpeedMulStart = 1.35f;
    [SerializeField] private float verticalSpeedMulEnd = 0.75f;

    [Header("Drop Pickup On Land")]
    [SerializeField] private GameObject pickupPrefab;
    [SerializeField] private bool spawnPickupOnLand = true;

    // Runtime only (settings)
    private float flightSpeed;
    private float arcHeight;

    // NEW: Ground motion (no arc)
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

        groundPos = start;
        groundStart = start;
        groundTarget = desired;

        // Wall pre-check: if line hits wall, clamp target to just outside wall and set bounceDir
        if (wallMask.value != 0)
        {
            Vector2 s = groundStart;
            Vector2 d = groundTarget;

            RaycastHit2D wall = Physics2D.Linecast(s, d, wallMask);
            if (wall.collider != null)
            {
                Vector2 incomingDir = (d - s);
                if (incomingDir.sqrMagnitude < 0.000001f) incomingDir = Vector2.right;
                incomingDir.Normalize();

                // Reflect gives proper bounce direction
                bounceDir = Vector2.Reflect(incomingDir, wall.normal).normalized;

                // Clamp target to outside the wall using normal
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

        // Ensure initial rendered position
        transform.position = groundPos;
    }

    private void Update()
    {
        if (!active) return;

        float baseSpeed = isBouncing ? currentBounceSpeed : flightSpeed;

        // progress based on ground movement ONLY
        float tCurrent = totalGroundDistance <= 0.0001f ? 1f : Vector3.Distance(groundStart, groundPos) / totalGroundDistance;
        tCurrent = Mathf.Clamp01(tCurrent);

        float speedMul = 1f;
        if (!isBouncing && easeOutOnVerticalThrows && arcPerp == Vector2.zero)
            speedMul = Mathf.Lerp(verticalSpeedMulStart, verticalSpeedMulEnd, tCurrent);

        float step = baseSpeed * speedMul * Time.deltaTime;

        // Move ground position straight towards groundTarget
        Vector3 nextGround = Vector3.MoveTowards(groundPos, groundTarget, step);

        // Apply a tiny inherited drift to ground motion (not to arc offset)
        nextGround += (Vector3)(inheritedVelocity * 0.05f * Time.deltaTime);
        nextGround.z = 0f;

        // compute t from ground only
        float t = totalGroundDistance <= 0.0001f ? 1f : Vector3.Distance(groundStart, nextGround) / totalGroundDistance;
        t = Mathf.Clamp01(t);

        // arc offset (visual) - does NOT accumulate
        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        Vector3 arcOffset = (Vector3)(arcPerp * arc);

        // render
        transform.position = nextGround + arcOffset;

        groundPos = nextGround;

        if (isBouncing)
        {
            currentBounceSpeed = Mathf.Lerp(currentBounceSpeed, 0, wallBounceDrag * Time.deltaTime);
        }

        // Arrived (use ground distance!)
        if (Vector2.Distance(groundPos, groundTarget) <= 0.05f)
        {
            if (!isBouncing && bounceDir != Vector2.zero)
            {
                StartBounce();
                return;
            }

            Land();
        }
    }

    private void StartBounce()
    {
        FX_SoundSystem.I?.PlayHit("Wall", firstHit: true);
        isBouncing = true;
        currentBounceSpeed = wallBounceSpeed;

        float bounceDistance = Random.Range(wallBounceDistanceMin, wallBounceDistanceMax);
        Vector2 wanted = (Vector2)groundPos + bounceDir * bounceDistance;

        // NEW: prevent bounce target from ending inside/behind wall
        if (wallMask.value != 0)
        {
            RaycastHit2D wall = Physics2D.Linecast(groundPos, wanted, wallMask);
            if (wall.collider != null)
            {
                // clamp to outside the wall
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

        // Ensure ground target isn't inside a wall -> push out
        Vector3 finalGround = groundTarget;

        if (wallMask.value != 0)
        {
            Collider2D wallCol = Physics2D.OverlapCircle(finalGround, 0.02f, wallMask);
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

        // Snap render position to final ground (no arc at landing)
        transform.position = finalGround;

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

        // Same rule as before: horizontal throws arc up, vertical throws straight
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            arcPerp = Vector2.up;
        else
            arcPerp = Vector2.zero;

        totalGroundDistance = Vector3.Distance(groundStart, groundTarget);
    }

    private void OnDrawGizmosSelected()
    {
        // This will only be meaningful during play, but harmless
        Gizmos.DrawWireSphere(groundTarget, impactRadius);
    }
}