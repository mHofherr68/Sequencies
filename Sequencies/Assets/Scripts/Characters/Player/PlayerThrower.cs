using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Handles throwing inventory items as projectiles using the mouse.
/// 
/// Responsibilities:
/// - Reads left mouse click input (ignores clicks on UI).
/// - Selects the currently selected inventory item (fallback: first available item with a projectile).
/// - Consumes one item from the inventory.
/// - Spawns the matching projectile prefab and initializes its flight (ThrowableFlight).
/// - Limits throw distance to a maximum range.
/// - Optionally inherits a portion of the player's current movement velocity.
/// </summary>
public class PlayerThrower : MonoBehaviour
{
    [Header("Projectile Prefabs (per ItemType)")]

    [Tooltip("Mapping from ItemType to its projectile prefab.")]
    [SerializeField] private ItemProjectileMapping[] projectilePrefabs;

    [Header("Throw")]

    [Tooltip("Cooldown between throws (seconds).")]
    [SerializeField] private float throwCooldown = 0.6f;

    [Tooltip("Spawn offset from the player in the throw direction (world units).")]
    [SerializeField] private float spawnOffset = 0.6f;

    [Header("Max Range")]

    [Tooltip("Maximum throw range from the player to the target position (world units).")]
    [SerializeField] private float maxThrowRange = 4f;

    [Header("Flight")]

    [Tooltip("Projectile flight speed.")]
    [SerializeField] private float flightSpeed = 10f;

    [Tooltip("Arc height for the throw animation (visual).")]
    [SerializeField] private float arcHeight = 0.02f;

    [Header("Inherit Player Motion")]

    [Tooltip("How much of the player's current velocity is inherited by the projectile.")]
    [SerializeField] private float inheritFactor = 0.15f;

    private float lastThrowTime;
    private Camera cam;
    private PlayerController movement;

    [Serializable]
    private struct ItemProjectileMapping
    {
        [Tooltip("Inventory item type.")]
        public ItemType type;

        [Tooltip("Projectile prefab spawned when this item is thrown.")]
        public GameObject projectilePrefab;
    }

    private void Awake()
    {
        cam = Camera.main;
        movement = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Prevent throwing when clicking UI (inventory etc.)
            if (IsPointerOverUI())
                return;

            // Cooldown check
            if (Time.time >= lastThrowTime + throwCooldown)
            {
                lastThrowTime = Time.time;
                Throw();
            }
        }
    }

    /// <summary>
    /// Returns true if the mouse is currently over a UI element.
    /// Used to prevent throwing while interacting with the inventory UI.
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject(-1);
    }

    /// <summary>
    /// Spawns and launches a projectile based on the selected inventory item.
    /// Uses a fallback item selection if the selected slot is empty.
    /// </summary>
    private void Throw()
    {
        if (cam == null) cam = Camera.main;
        if (InventoryManager.I == null) return;

        // Check if any item exists in the inventory at all
        bool anyItem = false;
        foreach (ItemType t in Enum.GetValues(typeof(ItemType)))
        {
            if (InventoryManager.I.GetCount(t) > 0)
            {
                anyItem = true;
                break;
            }
        }

        if (!anyItem)
        {
            InventoryManager.I.ShowMessage("Inventar ist leer!");
            return;
        }

        // Primary selection: currently selected slot
        ItemType toThrow = InventoryManager.I.SelectedItem;

        // Fallback: if selected item is empty, pick the first available item that has a projectile prefab
        if (InventoryManager.I.GetCount(toThrow) <= 0)
        {
            foreach (ItemType t in Enum.GetValues(typeof(ItemType)))
            {
                if (InventoryManager.I.GetCount(t) > 0 && GetProjectilePrefab(t) != null)
                {
                    toThrow = t;
                    break;
                }
            }
        }

        // Resolve projectile prefab for this item type
        GameObject prefab = GetProjectilePrefab(toThrow);
        if (prefab == null) return;

        // Consume inventory item before spawning
        if (!InventoryManager.I.TryConsume(toThrow, 1)) return;

        // Convert mouse position to world position
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        float zDist = -cam.transform.position.z;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, zDist));
        mouseWorld.z = 0f;

        // Clamp throw target to max range
        Vector2 from = transform.position;
        Vector2 to = mouseWorld;
        Vector2 delta = to - from;

        float maxRange = Mathf.Max(0f, maxThrowRange);

        if (maxRange > 0f && delta.sqrMagnitude > maxRange * maxRange)
        {
            delta = delta.normalized * maxRange;
            to = from + delta;
            mouseWorld = new Vector3(to.x, to.y, 0f);
        }

        // Spawn projectile in front of the player towards the target direction
        Vector2 dir = delta.sqrMagnitude > 0.00001f ? delta.normalized : Vector2.right;
        Vector3 spawnPos = transform.position + (Vector3)(dir * spawnOffset);

        GameObject proj = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Inherit part of player movement
        Vector2 inherited = Vector2.zero;
        if (movement != null)
            inherited = movement.CurrentVelocity * inheritFactor;

        // Initialize projectile flight behaviour
        var flight = proj.GetComponent<ThrowableFlight>();
        if (flight != null)
            flight.Init(mouseWorld, flightSpeed, arcHeight, inherited);
    }

    /// <summary>
    /// Returns the projectile prefab for a given ItemType.
    /// </summary>
    private GameObject GetProjectilePrefab(ItemType type)
    {
        if (projectilePrefabs == null) return null;

        for (int i = 0; i < projectilePrefabs.Length; i++)
            if (projectilePrefabs[i].type == type)
                return projectilePrefabs[i].projectilePrefab;

        return null;
    }
}