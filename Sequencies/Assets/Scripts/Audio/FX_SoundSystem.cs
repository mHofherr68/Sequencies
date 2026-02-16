/*using System;
using UnityEngine;

public class FX_SoundSystem : MonoBehaviour
{
    [Serializable]
    public struct TagSfx
    {
        public string tag;              // z.B. "Window"
        public AudioClip firstHit;      // z.B. Glasbruch
        public AudioClip repeatHit;     // z.B. Glas-Klirren
        [Range(0f, 1f)] public float volume;
    }

    public static FX_SoundSystem I { get; private set; }

    [SerializeField] private AudioSource source;
    [SerializeField] private TagSfx[] sfxByTag;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        //DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(transform.root.gameObject);

        if (source == null) source = GetComponent<AudioSource>();
    }

    public void PlayHit(string tag, bool firstHit)
    {
        if (source == null || string.IsNullOrEmpty(tag)) return;

        for (int i = 0; i < sfxByTag.Length; i++)
        {
            if (string.Equals(sfxByTag[i].tag, tag, StringComparison.OrdinalIgnoreCase))
            {
                var clip = firstHit ? sfxByTag[i].firstHit : sfxByTag[i].repeatHit;
                if (clip == null) return;

                float vol = sfxByTag[i].volume <= 0 ? 1f : sfxByTag[i].volume;
                source.PlayOneShot(clip, vol);
                return;
            }
        }
    }
}*/




using System;
using UnityEngine;

public class FX_SoundSystem : MonoBehaviour
{
    [Serializable]
    public struct SfxByTag
    {
        public string tag;            // z.B. "Window", "Wall", "Wood"
        public AudioClip firstHit;    // z.B. Window: Break
        public AudioClip repeatHit;   // z.B. Window: Hit
        [Range(0f, 1f)] public float volume;
    }

    public static FX_SoundSystem I { get; private set; }

    [Header("Output")]
    [SerializeField] private AudioSource source;

    [Header("SFX Library")]
    [SerializeField] private SfxByTag[] sfxByTag;

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
        {
            source = GetComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Spielt einen Hit-Sound basierend auf einem Tag (z.B. "Wall", "Window", "Wood").
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

            float vol = sfxByTag[i].volume <= 0f ? 1f : sfxByTag[i].volume;
            source.PlayOneShot(clip, vol);
            return;
        }
    }

    /// <summary>
    /// Spielt einen Hit-Sound basierend auf SurfaceType (Wood/Tile/Wall/...).
    /// Intern wird type.ToString() genutzt (muss zur Tag-Bezeichnung passen).
    /// </summary>
    public void PlayHit(SurfaceType type, bool firstHit)
    {
        PlayHit(type.ToString(), firstHit);
    }
}
