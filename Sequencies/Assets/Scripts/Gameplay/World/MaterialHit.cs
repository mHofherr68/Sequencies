using UnityEngine;

/// <summary>
/// Defines the possible surface materials that can be hit by projectiles.
/// 
/// These values are primarily used by the audio system (FX_SoundSystem)
/// to determine which sound effect should be played when an object
/// impacts a surface.
/// </summary>
public enum SurfaceType
{
    /// <summary>Wooden surface.</summary>
    Wood,

    /// <summary>Hard tile surface.</summary>
    Tile,

    /// <summary>Concrete or stone-like surface.</summary>
    Concrete,

    /// <summary>Grass or vegetation surface.</summary>
    Grass,

    /// <summary>Loose sand surface.</summary>
    Sand,

    /// <summary>Soft material (e.g., cloth, soft ground).</summary>
    Soft,

    /// <summary>Glass window surface.</summary>
    Window,

    /// <summary>Water surface.</summary>
    Water,

    /// <summary>Generic wall impact.</summary>
    Wall
}

/// <summary>
/// Component used to assign a <see cref="SurfaceType"/> to a collider.
/// 
/// When a projectile hits a surface with this component, the
/// <see cref="ThrowableFlight"/> system can read the material type and
/// trigger the appropriate sound effect via <see cref="FX_SoundSystem"/>.
/// </summary>
public class MaterialHit : MonoBehaviour
{
    [Tooltip("Surface type used for impact sound detection.")]
    public SurfaceType type = SurfaceType.Wood;
}