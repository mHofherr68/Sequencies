using UnityEngine;

public class ThrowableFlight : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float flightSpeed = 10f;
    [SerializeField] private float arcHeight = 0.02f;
    [SerializeField] private float impactRadius = 0.25f;

    [Header("Detection")]
    [SerializeField] private LayerMask surfaceMask = ~0; // default: Everything (im Inspector auf Surface stellen!)

    private Vector3 startPos;
    private Vector3 targetPos;
    private float totalDistance;
    private bool active;

    private Vector2 inheritedVelocity;
    private Vector2 arcPerp; // senkrecht zur Flugrichtung

    /// <summary>
    /// Startet den Flug zum Zielpunkt.
    /// </summary>
    public void Init(Vector3 targetWorldPos, float speed, float arc, Vector2 inherited)
    {
        startPos = transform.position;
        startPos.z = 0f;

        targetPos = targetWorldPos;
        targetPos.z = 0f;

        flightSpeed = Mathf.Max(0.01f, speed);
        arcHeight = Mathf.Max(0f, arc);

        inheritedVelocity = inherited;

        Vector2 dir = ((Vector2)targetPos - (Vector2)startPos);
        if (dir.sqrMagnitude < 0.00001f) dir = Vector2.right;
        dir.Normalize();

        // Arc wird seitlich zur Flugrichtung erzeugt (TopDown-freundlich)
        arcPerp = new Vector2(-dir.y, dir.x);

        totalDistance = Vector3.Distance(startPos, targetPos);
        active = true;
    }

    private void Update()
    {
        if (!active) return;

        // 1) Bewegung Richtung Ziel (flat)
        Vector3 current = transform.position;
        current.z = 0f;

        Vector3 next = Vector3.MoveTowards(current, targetPos, flightSpeed * Time.deltaTime);

        // 2) Fortschritt 0..1 (für Arc)
        float t = totalDistance <= 0.0001f ? 1f : Vector3.Distance(startPos, next) / totalDistance;
        t = Mathf.Clamp01(t);

        // 3) Arc seitlich zur Flugrichtung (0->max->0)
        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
        next += (Vector3)(arcPerp * arc);

        // 4) ganz leichte Vererbung der Player-Bewegung (subtil)
        next += (Vector3)(inheritedVelocity * 0.05f * Time.deltaTime);

        next.z = 0f;
        transform.position = next;

        // Ankunft
        if (Vector2.Distance(transform.position, targetPos) <= 0.05f)
            Land();
    }

    /*private void Land()
    {
        active = false;

        // Wir setzen final auf den Zielpunkt (damit der Stein "liegt")
        transform.position = targetPos;

        // Treffer am Landepunkt suchen (nur in SurfaceMask)
        Vector2 impactPoint = (Vector2)targetPos;
        Collider2D hit = Physics2D.OverlapCircle(impactPoint, impactRadius, surfaceMask);

        if (hit != null)
        {
            // 1) Spezialfall: Fenster zerbrechen (Script liegt am Parent GO_Window)
            var window = hit.GetComponentInParent<BreakableWindow>();
            if (window != null)
            {
                window.OnHit();
            }

            // 2) Debug: Material (optional)
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
    }*/
    private void Land()
    {
        active = false;

        transform.position = targetPos;

        Vector2 impactPoint = (Vector2)targetPos;
        Collider2D hit = Physics2D.OverlapCircle(impactPoint, impactRadius, surfaceMask);

        bool shouldDespawn = false;

        if (hit != null)
        {
            // Fenster special
            var window = hit.GetComponentInParent<BreakableWindow>();
            if (window != null)
            {
                window.OnHit();
                shouldDespawn = true; // Stein geht durchs Fenster / ist verloren
            }

            // Optional: Material-Log (für Debug)
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
        {
            Destroy(gameObject);
        }
        else
        {
            // Stein bleibt liegen (später: Pickup)
            // Optional: hier könntest du Flight/State-Komponenten deaktivieren
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(targetPos, impactRadius);
    }
}
