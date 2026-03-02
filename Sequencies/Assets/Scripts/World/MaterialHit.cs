using UnityEngine;

public enum SurfaceType
{
    Wood,
    Tile,
    Concrete,
    Grass,
    Sand,
    Soft,
    Window,
    Water,
    Wall
}

public class MaterialHit : MonoBehaviour
{
    public SurfaceType type = SurfaceType.Wood;
}
