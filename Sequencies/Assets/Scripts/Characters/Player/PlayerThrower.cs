using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerThrower : MonoBehaviour
{
    [Header("Projectile Prefabs (per ItemType)")]
    [SerializeField] private ItemProjectileMapping[] projectilePrefabs;

    [Header("Throw")]
    [SerializeField] private float throwCooldown = 0.6f;
    [SerializeField] private float spawnOffset = 0.6f;

    [Header("Max Range")]
    [SerializeField] private float maxThrowRange = 4f;

    [Header("Flight")]
    [SerializeField] private float flightSpeed = 10f;
    [SerializeField] private float arcHeight = 0.02f;

    [Header("Inherit Player Motion")]
    [SerializeField] private float inheritFactor = 0.15f;

    private float lastThrowTime;
    private Camera cam;
    private PlayerController movement;

    [Serializable]
    private struct ItemProjectileMapping
    {
        public ItemType type;
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
            if (IsPointerOverUI())
                return;

            if (Time.time >= lastThrowTime + throwCooldown)
            {
                lastThrowTime = Time.time;
                Throw();
            }
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject(-1);
    }

    private void Throw()
    {
        if (cam == null) cam = Camera.main;
        if (InventoryManager.I == null) return;

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

        ItemType toThrow = InventoryManager.I.SelectedItem;

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

        GameObject prefab = GetProjectilePrefab(toThrow);
        if (prefab == null) return;

        if (!InventoryManager.I.TryConsume(toThrow, 1)) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        float zDist = -cam.transform.position.z;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, zDist));
        mouseWorld.z = 0f;

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

        Vector2 dir = delta.sqrMagnitude > 0.00001f ? delta.normalized : Vector2.right;
        Vector3 spawnPos = transform.position + (Vector3)(dir * spawnOffset);

        GameObject proj = Instantiate(prefab, spawnPos, Quaternion.identity);

        Vector2 inherited = Vector2.zero;
        if (movement != null)
            inherited = movement.CurrentVelocity * inheritFactor;

        var flight = proj.GetComponent<ThrowableFlight>();
        if (flight != null)
            flight.Init(mouseWorld, flightSpeed, arcHeight, inherited);
    }

    private GameObject GetProjectilePrefab(ItemType type)
    {
        if (projectilePrefabs == null) return null;
        for (int i = 0; i < projectilePrefabs.Length; i++)
            if (projectilePrefabs[i].type == type)
                return projectilePrefabs[i].projectilePrefab;
        return null;
    }
}
