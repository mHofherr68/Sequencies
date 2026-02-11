using UnityEngine;

public enum SurfaceType
{
    Wood,
    Tile,
    Grass,
    Sand,
    Bed,
    Glass,
    Water,
    Wall
}

public class SurfaceMaterial : MonoBehaviour
{
    public SurfaceType type = SurfaceType.Wood;
}
