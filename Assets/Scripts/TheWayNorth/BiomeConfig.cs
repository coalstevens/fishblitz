using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeConfig", menuName = "Core/Biome Config")]
public class BiomeConfig : ScriptableObject
{
    public List<WeightedScene> ScenePool;
    public List<WeightedScene> Forks;
    public int PoolScenesBeforeFork;
}
