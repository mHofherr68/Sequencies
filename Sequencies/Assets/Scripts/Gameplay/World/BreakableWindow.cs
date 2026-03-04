using UnityEngine;

/// <summary>
/// Represents a breakable window that can react to projectile impacts.
/// 
/// Behaviour:
/// - Plays an impact sound using the object's tag.
/// - Spawns a temporary "noise light" at the hit position.
/// - Triggers a ghost noise event via the GhostSpawner singleton.
/// - Switches visual state from intact to broken (only once).
/// </summary>
public class BreakableWindow : MonoBehaviour
{
    [Header("Visuals")]
    [Tooltip("Root object containing the intact window visuals.")]
    [SerializeField] private GameObject intactRoot;

    [Tooltip("Root object containing the broken window visuals.")]
    [SerializeField] private GameObject brokenRoot;

    [Header("Noise Light (non static)")]
    [Tooltip("Prefab containing a temporary light used to visualize the noise event.")]
    [SerializeField] private GameObject noiseLightPrefab;

    /// <summary>
    /// Indicates whether the window has already been broken.
    /// </summary>
    private bool isBroken;

    /// <summary>
    /// Called when the window is hit by a projectile.
    /// Triggers sound, light, ghost noise and visual state change.
    /// </summary>
    public void OnHit()
    {
        // Play impact sound based on object tag
        FX_SoundSystem.I?.PlayHit(tag, firstHit: !isBroken);

        // Spawn temporary noise light
        if (noiseLightPrefab != null)
            Instantiate(noiseLightPrefab, transform.position, Quaternion.identity);

        // Trigger ghost noise target (singleton)
        GhostSpawner.I?.SpawnGhostTo(transform.position);

        // Switch visual state only once
        if (isBroken) return;

        isBroken = true;

        if (intactRoot != null)
            intactRoot.SetActive(false);

        if (brokenRoot != null)
            brokenRoot.SetActive(true);
    }
}