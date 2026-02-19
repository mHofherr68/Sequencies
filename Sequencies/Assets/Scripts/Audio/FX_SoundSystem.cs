using System;
using UnityEngine;

public class FX_SoundSystem : MonoBehaviour
{
    [Serializable]
    public struct SfxByTag
    {
        public string tag;
        public AudioClip firstHit;
        public AudioClip repeatHit;
        [Range(0f, 1f)] public float volume;
    }

    public static FX_SoundSystem I { get; private set; }

    [Header("Output")]
    [SerializeField] private AudioSource source;

    [Header("SFX Library")]
    [SerializeField] private SfxByTag[] sfxByTag;

    // Perceptual loudness curve
    private const float VolumeCurve = 2.2f;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        DontDestroyOnLoad(transform.root.gameObject);

        if (source == null)
            source = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Plays a hit sound based on a tag (e.g. "Wall", "Window", "Wood").
    /// Uses a perceptual loudness curve for more natural volume control.
    /// </summary>
    public void PlayHit(string tag, bool firstHit)
    {
        if (source == null) return;
        if (string.IsNullOrEmpty(tag)) return;

        for (int i = 0; i < sfxByTag.Length; i++)
        {
            if (!string.Equals(sfxByTag[i].tag, tag, StringComparison.OrdinalIgnoreCase))
                continue;

            AudioClip clip = firstHit ? sfxByTag[i].firstHit : sfxByTag[i].repeatHit;
            if (clip == null) return;

            float slider = Mathf.Clamp01(sfxByTag[i].volume);

            // Ear-like loudness
            float vol = Mathf.Pow(slider, VolumeCurve);

            source.PlayOneShot(clip, vol);
            return;
        }
    }

    /// <summary>
    /// Plays a hit sound using SurfaceType.
    /// Internally maps to the tag name.
    /// </summary>
    public void PlayHit(SurfaceType type, bool firstHit)
    {
        PlayHit(type.ToString(), firstHit);
    }
}

