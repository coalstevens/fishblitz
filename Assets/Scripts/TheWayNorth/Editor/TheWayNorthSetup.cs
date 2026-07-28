using UnityEditor;
using UnityEngine;

public static class TheWayNorthSetup
{
    private const string ConfigPath = "Assets/Settings/TheWayNorthConfig.asset";
    private const string WorldStatePrefabPath = "Assets/Core/Persistent/WorldState.prefab";

    [MenuItem("Tools/The Way North/Setup")]
    private static void Setup()
    {
        CreateConfigAsset();
        UpdateWorldStatePrefab();
        Debug.Log("The Way North setup complete.");
    }

    private static void CreateConfigAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TheWayNorthConfig>(ConfigPath);
        if (existing != null)
        {
            Debug.Log($"TheWayNorthConfig already exists at {ConfigPath}");
            return;
        }

        var config = ScriptableObject.CreateInstance<TheWayNorthConfig>();
        AssetDatabase.CreateAsset(config, ConfigPath);

        config.StartScene = SceneNames.CanyonStart;

        var canyonBiome = AssetDatabase.LoadAssetAtPath<BiomeConfig>("Assets/Resources/TheWayNorth/Canyon.asset");
        if (canyonBiome != null)
        {
            config.Biomes = new System.Collections.Generic.List<BiomeConfig> { canyonBiome };
        }
        else
        {
            Debug.LogError("Could not find Canyon BiomeConfig at Assets/Resources/TheWayNorth/Canyon.asset");
        }

        config.NetworkDepth = 2;

        var pool1 = new TheWayNorthConfig.DepthBiomePool
        {
            Depth = 1,
            PossibleBiomes = new System.Collections.Generic.List<TheWayNorthConfig.WeightedBiome>
            {
                new TheWayNorthConfig.WeightedBiome { Biome = canyonBiome, Weight = 1f }
            }
        };
        var pool2 = new TheWayNorthConfig.DepthBiomePool
        {
            Depth = 2,
            PossibleBiomes = new System.Collections.Generic.List<TheWayNorthConfig.WeightedBiome>
            {
                new TheWayNorthConfig.WeightedBiome { Biome = canyonBiome, Weight = 1f }
            }
        };
        config.DepthBiomePools = new System.Collections.Generic.List<TheWayNorthConfig.DepthBiomePool>
        {
            pool1, pool2
        };

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        Debug.Log($"Created TheWayNorthConfig at {ConfigPath}");
    }

    private static void UpdateWorldStatePrefab()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WorldStatePrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"WorldState prefab not found at {WorldStatePrefabPath}");
            return;
        }

        var existing = prefab.GetComponent<TheWayNorth>();
        if (existing != null)
        {
            Debug.Log("TheWayNorth component already exists on WorldState prefab");
            return;
        }

        var config = AssetDatabase.LoadAssetAtPath<TheWayNorthConfig>(ConfigPath);
        if (config == null)
        {
            Debug.LogError($"TheWayNorthConfig not found at {ConfigPath}. Run setup again after creating it.");
            return;
        }

        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            Debug.LogError("Failed to instantiate WorldState prefab");
            return;
        }

        var component = instance.AddComponent<TheWayNorth>();
        var serialized = new SerializedObject(component);
        serialized.FindProperty("_config").objectReferenceValue = config;
        serialized.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(instance, WorldStatePrefabPath);
        Object.DestroyImmediate(instance);

        Debug.Log($"Added TheWayNorth component to {WorldStatePrefabPath}");
    }
}
