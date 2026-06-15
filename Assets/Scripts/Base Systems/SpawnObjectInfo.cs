using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewSpawnObjectInfo", menuName = "Spawn Object Info")]
public class SpawnObjectInfo : ScriptableObject
{
    public List<GameObject> PrefabVariants;
    public List<TileBase> ValidTileTypes = new();
    public float Density = 0.05f;
    public bool SpawnNearbyOtherTypes = false;
    public int NearbyAdjacentDistance = 1;
    public List<GameObject> NearbyTypes = new();
    public bool UsePerlinNoise = false;
    public float PerlinNoiseScale = 0.1f;
    public float PerlinThreshold = 0.5f;
}
