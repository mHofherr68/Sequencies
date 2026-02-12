using UnityEngine;

public enum SurfaceType
{
    Wood,
    Tile,
    Grass,
    Sand,
    Soft,
    Window,
    Water,
    Wall
}

public class SurfaceMaterial : MonoBehaviour
{
    public SurfaceType type = SurfaceType.Wood;
}
