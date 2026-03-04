using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class NoiseLightLifetime : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Wie lange bleibt das Licht voll an, bevor es ausfadet?")]
    [SerializeField] private float holdTime = 1.25f;

    [Tooltip("Wie lange dauert das Fade-Out (bis 0), danach Destroy.")]
    [SerializeField] private float fadeOutTime = 0.75f;

    private Light2D light2D;
    private float startIntensity;
    private float t;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
        startIntensity = light2D.intensity;
    }

    private void OnEnable()
    {
        // falls intensity im Prefab geändert wurde
        startIntensity = light2D.intensity;
        t = 0f;
    }

    private void Update()
    {
        t += Time.deltaTime;

        // Phase 1: halten
        if (t < holdTime) return;

        // Phase 2: ausfaden
        float ft = Mathf.Max(0.01f, fadeOutTime);
        float k = Mathf.Clamp01((t - holdTime) / ft);

        light2D.intensity = Mathf.Lerp(startIntensity, 0f, k);

        if (k >= 1f)
            Destroy(gameObject);
    }
}