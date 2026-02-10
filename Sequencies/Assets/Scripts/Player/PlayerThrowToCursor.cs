/*using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrowToCursor : MonoBehaviour
{
    [SerializeField] private GameObject stonePrefab;

    [Header("Throw")]
    [SerializeField] private float throwCooldown = 0.6f;
    [SerializeField] private float spawnOffset = 0.5f;

    [Header("Flight")]
    [SerializeField] private float flightSpeed = 10f;
    [SerializeField] private float arcHeight = 0.3f;

    private float lastThrowTime;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= lastThrowTime + throwCooldown)
        {
            lastThrowTime = Time.time;
            Throw();
        }
    }

    private void Throw()
    {
        if (stonePrefab == null) return;
        if (cam == null) cam = Camera.main;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        mouseWorld.z = 0f;

        Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        Vector3 spawnPos = transform.position + (Vector3)(dir * spawnOffset);

        GameObject stone = Instantiate(stonePrefab, spawnPos, Quaternion.identity);

        var flight = stone.GetComponent<ThrowableFlight>();
        if (flight != null)
        {
            flight.Init(mouseWorld, flightSpeed, arcHeight);
        }
    }
}*/
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrowToCursor : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject stonePrefab;

    [Header("Throw")]
    [SerializeField] private float throwCooldown = 0.6f;
    [SerializeField] private float spawnOffset = 0.6f;

    [Header("Flight")]
    [SerializeField] private float flightSpeed = 10f;
    [SerializeField] private float arcHeight = 0.02f;

    [Header("Inherit Player Motion")]
    [SerializeField] private float inheritFactor = 0.15f; // 0.0 = nichts, 0.1–0.25 gut

    private float lastThrowTime;
    private Camera cam;
    private PlayerMovement movement;

    private void Awake()
    {
        cam = Camera.main;
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= lastThrowTime + throwCooldown)
        {
            lastThrowTime = Time.time;
            Throw();
        }
    }

    private void Throw()
    {
        if (stonePrefab == null) return;
        if (cam == null) cam = Camera.main;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        float zDist = -cam.transform.position.z; // wichtig für ScreenToWorld
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, zDist));
        mouseWorld.z = 0f;

        Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;

        Vector3 spawnPos = transform.position + (Vector3)(dir * spawnOffset);

        GameObject stone = Instantiate(stonePrefab, spawnPos, Quaternion.identity);

        // Player-Bewegung "vererben"
        Vector2 inherited = Vector2.zero;
        if (movement != null)
            inherited = movement.CurrentVelocity * inheritFactor;

        var flight = stone.GetComponent<ThrowableFlight>();
        if (flight != null)
        {
            flight.Init(mouseWorld, flightSpeed, arcHeight, inherited);
        }
    }
}
