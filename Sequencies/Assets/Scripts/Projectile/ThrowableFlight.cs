/*using UnityEngine;        Funktioniert gut, aber arc und flightspeed sind doppelt vorhanden
                         // und werden ignoriert

public class ThrowableFlight : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float flightSpeed = 10f;
    [SerializeField] private float arcHeight = 0.02f;
    [SerializeField] private float impactRadius = 0.25f;

    [Header("Detection")]
    [SerializeField] private LayerMask surfaceMask = ~0;
    [SerializeField] private LayerMask wallMask = 0;

    [Header("Wall Bounce")]
    [SerializeField] private float wallDistance = 0.05f;
    [SerializeField] private float wallBounceDistanceMin = 0.25f;
    [SerializeField] private float wallBounceDistanceMax = 0.55f;
    [SerializeField] private float wallBounceSpeed = 8f;   // ? jetzt bewusst niedriger
    [SerializeField] private float wallBounceDrag = 4f;    // ? NEU (bremst Bounce sichtbar)

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

                var surface = wall.collider.GetComponent<SurfaceMaterial>();
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

        float speed = isBouncing ? currentBounceSpeed : flightSpeed;

        Vector3 current = transform.position;
        Vector3 next = Vector3.MoveTowards(current, targetPos, speed * Time.deltaTime);

        float t = totalDistance <= 0.0001f ? 1f : Vector3.Distance(startPos, next) / totalDistance;
        t = Mathf.Clamp01(t);

        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        next += (Vector3)(arcPerp * arc);
        next += (Vector3)(inheritedVelocity * 0.05f * Time.deltaTime);

        next.z = 0f;
        transform.position = next;

        // ? Bounce Drag anwenden
        if (isBouncing)
        {
            currentBounceSpeed = Mathf.Lerp(
                currentBounceSpeed,
                0,
                wallBounceDrag * Time.deltaTime);
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
        isBouncing = true;

        currentBounceSpeed = wallBounceSpeed;

        float bounceDistance = Random.Range(
            wallBounceDistanceMin,
            wallBounceDistanceMax);

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

            var surface = hit.GetComponent<SurfaceMaterial>();
            if (surface != null)
                Debug.Log($"Impact: {surface.type} ({hit.gameObject.name})");
            else
                Debug.Log($"Impact: (no SurfaceMaterial) Tag={hit.tag} ({hit.gameObject.name})");
        }
        else
        {
            Debug.Log("Impact: (nothing) -> not on Surface layer / outside mask");
        }

        if (shouldDespawn)
            Destroy(gameObject);
    }

    private void RecomputeArc(Vector3 from, Vector3 to)
    {
        startPos = from;
        targetPos = to;

        Vector2 dir = ((Vector2)targetPos - (Vector2)startPos);
        if (dir.sqrMagnitude < 0.00001f) dir = Vector2.right;
        dir.Normalize();

        arcPerp = new Vector2(-dir.y, dir.x);
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

    // ?? Runtime only
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

        // ? SINGLE SOURCE OF TRUTH
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

                var surface = wall.collider.GetComponent<SurfaceMaterial>();
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

        float speed = isBouncing ? currentBounceSpeed : flightSpeed;

        Vector3 current = transform.position;
        Vector3 next = Vector3.MoveTowards(current, targetPos, speed * Time.deltaTime);

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

            var surface = hit.GetComponent<SurfaceMaterial>();
            if (surface != null)
                Debug.Log($"Impact: {surface.type} ({hit.gameObject.name})");
            else
                Debug.Log($"Impact: (no SurfaceMaterial) Tag={hit.tag} ({hit.gameObject.name})");
        }
        else
        {
            Debug.Log("Impact: (nothing) -> not on Surface layer / outside mask");
        }

        if (shouldDespawn)
            Destroy(gameObject);
    }

    private void RecomputeArc(Vector3 from, Vector3 to)
    {
        startPos = from;
        targetPos = to;

        Vector2 dir = ((Vector2)targetPos - (Vector2)startPos);
        if (dir.sqrMagnitude < 0.00001f) dir = Vector2.right;
        dir.Normalize();

        arcPerp = new Vector2(-dir.y, dir.x);
        totalDistance = Vector3.Distance(startPos, targetPos);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(targetPos, impactRadius);
    }
}
