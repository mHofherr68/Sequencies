/*using UnityEngine;

public class ThrowableFlight : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float flightSpeed = 10f;      // Welt-Einheiten pro Sekunde
    [SerializeField] private float arcHeight = 0.3f;        // visuelle Bogenhöhe (0 = gerade)
    [SerializeField] private float impactRadius = 0.25f;    // wie groß die "Landepunkt-Kollision" ist

    [Header("Detection")]
    [SerializeField] private LayerMask surfaceMask;         // optional: setz später "Surfaces"-Layer
    [SerializeField] private string[] surfaceTags;          // optional: z.B. Bed, Tile, Wood, Glass

    private Vector3 startPos;
    private Vector3 targetPos;
    private float totalDistance;

    private bool active;

    public void Init(Vector3 targetWorldPos, float speed, float arc)
    {
        startPos = transform.position;
        targetPos = targetWorldPos;
        targetPos.z = 0f;

        flightSpeed = Mathf.Max(0.01f, speed);
        arcHeight = Mathf.Max(0f, arc);

        totalDistance = Vector3.Distance(startPos, targetPos);
        active = true;
    }

    private void Update()
    {
        if (!active) return;

        // Bewegung Richtung Ziel
        Vector3 current = transform.position;
        Vector3 next = Vector3.MoveTowards(current, targetPos, flightSpeed * Time.deltaTime);

        // Fortschritt 0..1 für Fake-Bogen
        float traveled = totalDistance <= 0.0001f ? 1f : Vector3.Distance(startPos, next) / totalDistance;
        float arc = Mathf.Sin(traveled * Mathf.PI) * arcHeight; // 0->max->0

        // Wir verschieben nur visuell in Y (TopDown): "Bogen" als kleiner Offset
        // Das ist ein Fake – aber wirkt gut.
        next.y += arc;

        transform.position = next;

        // Ankunft prüfen (ohne arc-offset vergleichen, deshalb nur XY Distanz zum Ziel)
        Vector2 flatPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 flatTarget = new Vector2(targetPos.x, targetPos.y);
        if (Vector2.Distance(flatPos, flatTarget) <= 0.05f)
        {
            Land();
        }
    }

    private void Land()
    {
        active = false;

        // "Impact" am Landepunkt bestimmen (Overlap Circle)
        Collider2D hit = null;

        // Wenn du surfaceMask noch nicht nutzt, setz ihn im Inspector auf "Everything"
        hit = Physics2D.OverlapCircle(targetPos, impactRadius, surfaceMask);

        if (hit != null)
        {
            string tag = hit.tag;
            Debug.Log($"Impact on: {hit.gameObject.name} (Tag: {tag})");

            // Hier später Sound dispatchen:
            // PlaySoundForTag(tag);
        }
        else
        {
            Debug.Log("Impact on: (nothing) -> default ground");
        }

        // Für morgen: Stein bleibt liegen. (Später: aufhebbar / despawn bei Teich/Fenster)
        transform.position = targetPos;
        // Optional: wenn du willst, dass er „liegenbleibt“ ohne weitere Updates, reicht das.
        // Kein Destroy.
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(targetPos, impactRadius);
    }
}*/
/*using UnityEngine;

public class ThrowableFlight : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float flightSpeed = 10f;
    [SerializeField] private float arcHeight = 0.02f;
    [SerializeField] private float impactRadius = 0.25f;

    [Header("Detection (später)")]
    [SerializeField] private LayerMask surfaceMask = ~0; // default: Everything

    private Vector3 startPos;
    private Vector3 targetPos;
    private float totalDistance;
    private bool active;

    private Vector2 inheritedVelocity; // ? Player-Movement Anteil

    // ? Neue Init-Signatur mit inheritedVelocity
    public void Init(Vector3 targetWorldPos, float speed, float arc, Vector2 inherited)
    {
        startPos = transform.position;
        targetPos = targetWorldPos;
        targetPos.z = 0f;

        flightSpeed = Mathf.Max(0.01f, speed);
        arcHeight = Mathf.Max(0f, arc);

        inheritedVelocity = inherited;

        totalDistance = Vector3.Distance(startPos, targetPos);
        active = true;
    }

    private void Update()
    {
        if (!active) return;

        Vector3 current = transform.position;

        // Grundbewegung Richtung Ziel
        Vector3 next = Vector3.MoveTowards(current, targetPos, flightSpeed * Time.deltaTime);

        // Kleiner "Mitnahme"-Effekt aus Player-Bewegung (TopDown)
        next += (Vector3)(inheritedVelocity * Time.deltaTime);

        // Fake-Bogen (sehr klein bei dir, z.B. 0.02)
        float traveled = totalDistance <= 0.0001f ? 1f : Vector3.Distance(startPos, next) / totalDistance;
        float arc = Mathf.Sin(traveled * Mathf.PI) * arcHeight;
        next.y += arc;

        next.z = 0f;
        transform.position = next;

        // Ankunft prüfen
        Vector2 flatPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 flatTarget = new Vector2(targetPos.x, targetPos.y);
        if (Vector2.Distance(flatPos, flatTarget) <= 0.05f)
        {
            Land();
        }
    }

    private void Land()
    {
        active = false;

        Collider2D hit = Physics2D.OverlapCircle(targetPos, impactRadius, surfaceMask);

        if (hit != null)
            Debug.Log($"Impact on: {hit.gameObject.name} (Tag: {hit.tag})");
        else
            Debug.Log("Impact on: (nothing) -> default ground");

        transform.position = targetPos;
    }
}*/
using UnityEngine;

public class ThrowableFlight : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float flightSpeed = 10f;
    [SerializeField] private float arcHeight = 0.02f;
    [SerializeField] private float impactRadius = 0.25f;

    [Header("Detection")]
    [SerializeField] private LayerMask surfaceMask = ~0;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float totalDistance;
    private bool active;

    private Vector2 inheritedVelocity;
    private Vector2 arcPerp; // senkrecht zur Flugrichtung

    public void Init(Vector3 targetWorldPos, float speed, float arc, Vector2 inherited)
    {
        startPos = transform.position;
        startPos.z = 0f;

        targetPos = targetWorldPos;
        targetPos.z = 0f;

        flightSpeed = Mathf.Max(0.01f, speed);
        arcHeight = Mathf.Max(0f, arc);

        inheritedVelocity = inherited;

        Vector2 dir = ((Vector2)targetPos - (Vector2)startPos).normalized;
        arcPerp = new Vector2(-dir.y, dir.x); // senkrecht zur Richtung

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

        // 2) Fortschritt für Arc (0..1)
        float t = totalDistance <= 0.0001f ? 1f : Vector3.Distance(startPos, next) / totalDistance;
        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;

        // 3) Arc seitlich zur Flugrichtung (nicht immer +Y)
        next += (Vector3)(arcPerp * arc);

        // 4) Inherit nur sehr subtil (sonst zerstört es das Zielgefühl)
        next += (Vector3)(inheritedVelocity * 0.05f * Time.deltaTime);

        next.z = 0f;
        transform.position = next;

        if (Vector2.Distance(transform.position, targetPos) <= 0.05f)
            Land();
    }

    /*private void Land()
    {
        active = false;

        Collider2D hit = Physics2D.OverlapCircle(targetPos, impactRadius, surfaceMask);

        if (hit != null)
            Debug.Log($"Impact on: {hit.gameObject.name} (Tag: {hit.tag})");
        else
            Debug.Log("Impact on: (nothing) -> default ground");

        transform.position = targetPos;
    }*/
    /*private void Land()
    {
        active = false;

        // 1) Auf den Landepunkt snappen (damit die Probe garantiert stimmt)
        transform.position = targetPos;

        // 2) Genau dort prüfen
        Vector2 impactPoint = (Vector2)transform.position;

        Collider2D hit = Physics2D.OverlapCircle(impactPoint, impactRadius, surfaceMask);

        if (hit != null)
            Debug.Log($"Impact on: {hit.gameObject.name} (Tag: {hit.tag})");
        else
            Debug.Log("Impact on: (nothing) -> default ground");
    }*/
    private void Land()
    {
        active = false;

        transform.position = targetPos; // sicher am Ziel

        Vector2 impactPoint = (Vector2)transform.position;
        Collider2D hit = Physics2D.OverlapCircle(impactPoint, impactRadius, surfaceMask);

        if (hit != null)
        {
            var surface = hit.GetComponent<SurfaceMaterial>();
            if (surface != null)
                Debug.Log($"Impact: {surface.type}  ({hit.gameObject.name})");
            else
                Debug.Log($"Impact: (no SurfaceMaterial) Tag={hit.tag}  ({hit.gameObject.name})");
        }
        else
        {
            Debug.Log("Impact: (nothing) -> not on Surface layer");
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(targetPos, impactRadius);
    }
}
