using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls the lifetime of a temporary noise light.
/// 
/// Behaviour:
/// 1. The light stays at full intensity for a configurable hold duration.
/// 2. After that, the intensity fades smoothly to zero.
/// 3. Once the fade-out is complete, the GameObject is destroyed.
/// 
/// Typically used for sound visualization effects (e.g., clap, impact, door interaction).
/// </summary>
[RequireComponent(typeof(Light2D))]
public class NoiseLightLifetime : MonoBehaviour
{
    [Header("Timing")]

    [Tooltip("Duration in seconds the light stays at full intensity before fading out.")]
    [SerializeField] private float holdTime = 1.25f;

    [Tooltip("Duration in seconds for the fade-out until the light reaches zero intensity.")]
    [SerializeField] private float fadeOutTime = 0.75f;

    /// <summary>
    /// Reference to the Light2D component.
    /// </summary>
    private Light2D light2D;

    /// <summary>
    /// Initial light intensity at spawn.
    /// </summary>
    private float startIntensity;

    /// <summary>
    /// Internal timer used for hold and fade phases.
    /// </summary>
    private float t;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
        startIntensity = light2D.intensity;
    }

    private void OnEnable()
    {
        // Reset values when the object becomes active (important for reused prefabs)
        startIntensity = light2D.intensity;
        t = 0f;
    }

    private void Update()
    {
        t += Time.deltaTime;

        // Phase 1: hold full intensity
        if (t < holdTime)
            return;

        // Phase 2: fade out
        float ft = Mathf.Max(0.01f, fadeOutTime);
        float k = Mathf.Clamp01((t - holdTime) / ft);

        light2D.intensity = Mathf.Lerp(startIntensity, 0f, k);

        // Destroy object once fade-out is complete
        if (k >= 1f)
            Destroy(gameObject);
    }
}