using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    public enum SceneType { Inside, Outside }

    [SerializeField] private BootstrapperConfig _config;
    [SerializeField] private SceneType _sceneType;

    private void Awake()
    {
        InstantiatePrefabs(_config.PersistentPrefabs, persistent: true);
        InstantiatePrefabs(_config.AllScenePrefabs);
        InstantiatePrefabs(_sceneType == SceneType.Inside ? _config.InsidePrefabs : _config.OutsidePrefabs);
        Destroy(gameObject);
    }

    private void InstantiatePrefabs(GameObject[] prefabs, bool persistent = false)
    {
        if (prefabs == null) return;
        foreach (var prefab in prefabs)
        {
            if (prefab == null) continue;
            if (persistent && PersistentExists(prefab.name)) continue;
            var instance = Instantiate(prefab);
            if (persistent)
                DontDestroyOnLoad(instance);
        }
    }

    private static bool PersistentExists(string prefabName)
    {
        if (GameObject.Find(prefabName) != null) return true;
        if (GameObject.Find(prefabName + "(Clone)") != null) return true;
        return false;
    }
}
