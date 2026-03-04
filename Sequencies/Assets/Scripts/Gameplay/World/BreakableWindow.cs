/*using UnityEngine;

public class BreakableWindow : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject intactRoot;
    [SerializeField] private GameObject brokenRoot;

    private bool isBroken;

    public void OnHit()
    {
        // Zentraler SFX-Call: first vs repeat
        FX_SoundSystem.I?.PlayHit(tag, firstHit: !isBroken);

        // Sprite-Zustand nur einmal wechseln
        if (isBroken) return;

        isBroken = true;
        if (intactRoot != null) intactRoot.SetActive(false);
        if (brokenRoot != null) brokenRoot.SetActive(true);
    }
}*/
using UnityEngine;

public class BreakableWindow : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject intactRoot;
    [SerializeField] private GameObject brokenRoot;

    [Header("Noise Light (non static)")]
    [SerializeField] private GameObject noiseLightPrefab;

    private bool isBroken;

    public void OnHit()
    {
        // Sound
        FX_SoundSystem.I?.PlayHit(tag, firstHit: !isBroken);

        // Non-static Noise Light
        if (noiseLightPrefab != null)
            Instantiate(noiseLightPrefab, transform.position, Quaternion.identity);

        // Ghost noise target (Singleton)
        GhostSpawner.I?.SpawnGhostTo(transform.position);

        // Sprite-Zustand nur einmal wechseln
        if (isBroken) return;

        isBroken = true;
        if (intactRoot != null) intactRoot.SetActive(false);
        if (brokenRoot != null) brokenRoot.SetActive(true);
    }
}
