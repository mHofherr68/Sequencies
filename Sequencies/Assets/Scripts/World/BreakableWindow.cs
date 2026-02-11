using UnityEngine;

public class BreakableWindow : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject intactRoot;
    [SerializeField] private GameObject brokenRoot;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private AudioClip hitSound;

    private bool isBroken;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void OnHit()
    {
        // Sound immer abspielen
        if (audioSource != null)
        {
            if (!isBroken && breakSound != null)
                audioSource.PlayOneShot(breakSound);
            else if (isBroken && hitSound != null)
                audioSource.PlayOneShot(hitSound);
        }

        // Zustand nur einmal ändern
        if (isBroken) return;
        Break();
    }

    private void Break()
    {
        isBroken = true;

        if (intactRoot != null) intactRoot.SetActive(false);
        if (brokenRoot != null) brokenRoot.SetActive(true);
    }
}
