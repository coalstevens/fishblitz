using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TheWayNorthConfig", menuName = "Core/The Way North Config")]
public class TheWayNorthConfig : ScriptableObject
{
    [System.Serializable]
    public class WeightedBiome
    {
        public BiomeConfig Biome;
        [Range(0f, 1f)] public float Weight;
    }

    [System.Serializable]
    public class DepthBiomePool
    {
        public int Depth;
        public List<WeightedBiome> PossibleBiomes;
    }

    [Header("Config")]
    public SceneNames StartScene;
    public List<BiomeConfig> Biomes;

    [Header("Network Depth")]
    public int NetworkDepth = 1;
    public List<DepthBiomePool> DepthBiomePools;

    [Header("Debug")]
    public bool AlwaysRegenerateNetwork = false;

    public SceneNames PickWeighted(List<WeightedScene> entries)
    {
        float totalWeight = 0f;
        foreach (var entry in entries)
            totalWeight += entry.Weight;

        float roll = Random.Range(0f, totalWeight);
        foreach (var entry in entries)
        {
            roll -= entry.Weight;
            if (roll <= 0f)
                return entry.Scene;
        }

        return entries[entries.Count - 1].Scene;
    }

    public BiomeConfig PickBiome(List<WeightedBiome> biomes)
    {
        float totalWeight = 0f;
        foreach (var b in biomes)
            totalWeight += b.Weight;

        float roll = Random.Range(0f, totalWeight);
        foreach (var b in biomes)
        {
            roll -= b.Weight;
            if (roll <= 0f)
                return b.Biome;
        }

        return biomes[biomes.Count - 1].Biome;
    }

    public List<WeightedBiome> GetPossibleBiomesForDepth(int depth)
    {
        DepthBiomePool pool = DepthBiomePools.Find(p => p.Depth == depth);
        if (pool == null)
        {
            Debug.LogError($"GetPossibleBiomesForDepth: No DepthBiomePool for depth {depth}");
            return new List<WeightedBiome>();
        }
        return pool.PossibleBiomes;
    }

    public BiomeConfig FindBiomeConfig(string name)
    {
        foreach (var biome in Biomes)
            if (biome.name == name) return biome;
        return null;
    }
}
