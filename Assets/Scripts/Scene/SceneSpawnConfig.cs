using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneSpawnConfig", menuName = "Core/Scene Spawn Config")]
public class SceneSpawnConfig : ScriptableObject
{
    [Header("Scene Spawns")]
    [Tooltip("First entry in list is used as default spawn for scenes not listed below")]
    [SerializeField] private List<SceneSpawnEntry> _sceneSpawns = new();

    [System.Serializable]
    public struct SceneSpawnEntry
    {
        public SceneNames scene;
        public List<NamedSpawn> spawns;
    }

    [System.Serializable]
    public struct NamedSpawn
    {
        public string name;
        public Vector3 position;
    }

    public Vector3 GetSpawnPosition(SceneNames scene, string spawnName)
    {
        foreach (var entry in _sceneSpawns)
        {
            if (entry.scene == scene)
            {
                foreach (var spawn in entry.spawns)
                {
                    if (spawn.name == spawnName)
                        return spawn.position;
                }
                return GetDefaultSpawnPosition(entry);
            }
        }
        if (_sceneSpawns.Count > 0)
            return GetDefaultSpawnPosition(_sceneSpawns[0]);
        return Vector3.zero;
    }

    private Vector3 GetDefaultSpawnPosition(SceneSpawnEntry entry)
    {
        if (entry.spawns.Count > 0)
            return entry.spawns[0].position;
        return Vector3.zero;
    }

    public string GetDefaultSpawnName(SceneNames scene)
    {
        foreach (var entry in _sceneSpawns)
        {
            if (entry.scene == scene && entry.spawns.Count > 0)
                return entry.spawns[0].name;
        }
        return _sceneSpawns.Count > 0 && _sceneSpawns[0].spawns.Count > 0 
            ? _sceneSpawns[0].spawns[0].name 
            : null;
    }
}
