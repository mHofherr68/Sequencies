using UnityEngine;

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
}
