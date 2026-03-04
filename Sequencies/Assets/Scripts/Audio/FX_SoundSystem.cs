using System;
using UnityEngine;

/// <summary>
/// Centralized sound effect system for gameplay events.
/// 
/// This system maps string tags (e.g. "Wall", "Window", "Wood") to
/// audio clips and plays the correct sound depending on whether the
/// interaction is the first hit or a repeated hit.
/// 
/// The system is implemented as a persistent singleton so it can be
/// accessed globally through FX_SoundSystem.I.
/// </summary>
public class FX_SoundSystem : MonoBehaviour
{
    /// <summary>
    /// Defines a sound configuration for a specific tag.
    /// Each tag can have a different clip for the first hit
    /// and for repeated hits.
    /// </summary>
    [Serializable]
    public struct SfxByTag
    {
        [Tooltip("Tag identifier used to trigger this sound (e.g. 'Wall', 'Window', 'Wood').")]
        public string tag;

        [Tooltip("Audio clip played the first time the object is hit.")]
        public AudioClip firstHit;

        [Tooltip("Audio clip played on repeated hits after the first interaction.")]
        public AudioClip repeatHit;

        [Tooltip("Base volume of this sound (perceptual loudness curve applied internally).")]
        [Range(0f, 1f)] public float volume;
    }

    /// <summary>
    /// Global singleton instance for easy access from gameplay systems.
    /// </summary>
    public static FX_SoundSystem I { get; private set; }

    [Header("Output")]

    [Tooltip("AudioSource used to play all sound effects.")]
    [SerializeField] private AudioSource source;

    [Header("SFX Library")]

    [Tooltip("Collection of sound mappings used by the system.")]
    [SerializeField] private SfxByTag[] sfxByTag;

    /// <summary>
    /// Perceptual loudness curve exponent.
    /// Converts linear slider values into a more natural
    /// human-perceived loudness response.
    /// </summary>
    private const float VolumeCurve = 2.2f;

    /// <summary>
    /// Initializes the singleton instance and ensures the system
    /// persists across scene loads.
    /// </summary>
    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;

        // Make the entire root persistent across scene changes
        DontDestroyOnLoad(transform.root.gameObject);

        if (source == null)
            source = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Plays a sound effect associated with a specific tag.
    /// 
    /// The system searches the configured sound library for the matching tag,
    /// selects either the first-hit or repeat-hit clip, and plays it using
    /// a perceptual loudness curve for natural volume scaling.
    /// </summary>
    /// <param name="tag">
    /// Identifier used to select the correct sound entry
    /// (for example: "Wall", "Window", "Wood").
    /// </param>
    /// <param name="firstHit">
    /// True if the interaction is the first hit,
    /// false if it is a repeated interaction.
    /// </param>
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

            // Apply perceptual loudness curve
            float vol = Mathf.Pow(slider, VolumeCurve);

            source.PlayOneShot(clip, vol);
            return;
        }
    }

    /// <summary>
    /// Plays a hit sound using a SurfaceType enumeration.
    /// 
    /// Internally converts the enum value to its string name
    /// and forwards the request to the tag-based PlayHit method.
    /// </summary>
    /// <param name="type">SurfaceType enum used to determine the sound mapping.</param>
    /// <param name="firstHit">Indicates whether this is the first interaction.</param>
    public void PlayHit(SurfaceType type, bool firstHit)
    {
        PlayHit(type.ToString(), firstHit);
    }
}
