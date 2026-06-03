using System.Collections.Generic;
using OysterUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "CanyonManager", menuName = "Core/Canyon Manager")]
public class CanyonManager : Store
{
    [System.Serializable]
    public class BiomeConfig
    {
        public string BiomeName;
        public List<SceneNames> ScenePool;
        public List<SpecialSceneEntry> SpecialScenes;
    }

    [System.Serializable]
    public class SpecialSceneEntry
    {
        public SceneNames Scene;
        [Range(0, 100)] public float Weight;
    }

    [Header("Config")]
    public SceneNames CanyonStart;
    public PlayerData PlayerData;
    public List<BiomeConfig> Biomes;

    [SerializeField] private Logger _logger = new();

    [System.NonSerialized] private List<SceneNames> _visitedScenes = new();
    [System.NonSerialized] private int _currentSceneIndex = -1;
    [System.NonSerialized] private Dictionary<string, Queue<SceneNames>> _sceneQueues = new();
    [System.NonSerialized] private Dictionary<string, string> _exitToEntrance = new();
    [System.NonSerialized] private Dictionary<string, string> _entranceToExit = new();
    [System.NonSerialized] private bool _isActive;

    private static string _pendingNodeId;
    private static string _pendingSpawnLabel;
    private static string _pendingSourceKey;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_isActive && scene.name == CanyonStart.ToString())
        {
            StartRun();
            LogNetwork();
        }
    }

    public void TakeExit(string exitId, string transitionLabel, string targetBiome)
    {
        if (string.IsNullOrEmpty(targetBiome))
        {
            Debug.LogError($"TakeExit: Exit '{exitId}' in current scene has no targetBiome configured");
            return;
        }

        if (!_isActive)
            StartRun();

        string currentScene = _visitedScenes[_currentSceneIndex].ToString();
        string key = $"{currentScene}|{exitId}";

        if (_exitToEntrance.TryGetValue(key, out string target))
        {
            string[] parts = target.Split('|');
            string targetSceneName = parts[0];
            string entranceId = parts[1];

            SceneNames targetScene = ParseSceneName(targetSceneName);
            _currentSceneIndex = _visitedScenes.IndexOf(targetScene);

            _logger.Info($"{currentScene}|{exitId}:{transitionLabel} → {targetSceneName} (entrance:{entranceId} spawn:{transitionLabel})");
            LogNetwork();

            _pendingNodeId = entranceId;
            _pendingSpawnLabel = transitionLabel;
            SceneManager.sceneLoaded += OnSpawnResolve;
            SmoothSceneManager.LoadScene(targetSceneName);
        }
        else
        {
            SceneNames newScene = PickNextScene(targetBiome);

            _visitedScenes.Add(newScene);
            _currentSceneIndex = _visitedScenes.Count - 1;

            _logger.Info($"Generated {newScene} from biome '{targetBiome}' ({currentScene}|{exitId}:{transitionLabel})");
            LogNetwork();

            _pendingSourceKey = key;
            _pendingSpawnLabel = transitionLabel;
            SceneManager.sceneLoaded += OnSpawnResolve;
            SmoothSceneManager.LoadScene(newScene.ToString());
        }
    }

    public void UseEntrance(string entranceId, string transitionLabel)
    {
        if (!_isActive) return;

        string currentScene = _visitedScenes[_currentSceneIndex].ToString();
        string key = $"{currentScene}|{entranceId}";

        if (!_entranceToExit.TryGetValue(key, out string source))
        {
            Debug.LogError($"Entrance '{entranceId}' in '{currentScene}' has no linked exit");
            return;
        }

        string[] parts = source.Split('|');
        string sourceSceneName = parts[0];
        string exitId = parts[1];

        SceneNames sourceScene = ParseSceneName(sourceSceneName);
        _currentSceneIndex = _visitedScenes.IndexOf(sourceScene);

        if (sourceScene == CanyonStart)
            _isActive = false;

        _logger.Info($"{currentScene}|{entranceId}:{transitionLabel} ← {sourceSceneName} (exit:{exitId} spawn:{transitionLabel})");
        LogNetwork();

        _pendingNodeId = exitId;
        _pendingSpawnLabel = transitionLabel;
        SceneManager.sceneLoaded += OnSpawnResolve;
        SmoothSceneManager.LoadScene(sourceSceneName);
    }

    private void StartRun()
    {
        _visitedScenes.Clear();
        _exitToEntrance.Clear();
        _entranceToExit.Clear();
        _sceneQueues.Clear();

        _visitedScenes.Add(CanyonStart);
        _currentSceneIndex = 0;

        foreach (var biome in Biomes)
        {
            var shuffled = new List<SceneNames>(biome.ScenePool);
            Shuffle(shuffled);
            var queue = new Queue<SceneNames>();
            foreach (var scene in shuffled)
                queue.Enqueue(scene);
            _sceneQueues[biome.BiomeName] = queue;
        }

        _isActive = true;

        _logger.Info($"Canyon run started at {CanyonStart}");
    }

    private SceneNames PickNextScene(string biomeName)
    {
        BiomeConfig config = Biomes.Find(b => b.BiomeName == biomeName);
        if (config == null)
        {
            Debug.LogError($"PickNextScene: No BiomeConfig named '{biomeName}'. " +
                           $"Check CanyonExit._targetBiome on the Exit GO in the current scene.");
            return CanyonStart;
        }

        float roll = Random.Range(0f, 100f);
        foreach (var special in config.SpecialScenes)
        {
            if (roll < special.Weight)
                return special.Scene;
            roll -= special.Weight;
        }

        if (_sceneQueues.TryGetValue(biomeName, out var queue) && queue.Count > 0)
            return queue.Dequeue();

        ReplenishQueue(config, biomeName);
        return _sceneQueues[biomeName].Dequeue();
    }

    private void ReplenishQueue(BiomeConfig config, string biomeName)
    {
        var shuffled = new List<SceneNames>(config.ScenePool);
        Shuffle(shuffled);
        var queue = new Queue<SceneNames>();
        foreach (var scene in shuffled)
            queue.Enqueue(scene);
        _sceneQueues[biomeName] = queue;
    }

    private static void OnSpawnResolve(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSpawnResolve;
        string targetId = _pendingNodeId;
        string label = _pendingSpawnLabel;
        string sourceKey = _pendingSourceKey;
        _pendingNodeId = null;
        _pendingSpawnLabel = null;
        _pendingSourceKey = null;

        CanyonManager mgr = Get<CanyonManager>();

        if (string.IsNullOrEmpty(targetId) && string.IsNullOrEmpty(sourceKey)) return;

        if (!string.IsNullOrEmpty(sourceKey))
        {
            foreach (var entrance in FindObjectsByType<CanyonEntrance>(FindObjectsSortMode.None))
            {
                CanyonSpawn spawn = entrance.GetSpawn(label);
                if (spawn == null) continue;

                string sceneKey = $"{scene.name}|{entrance.EntranceId}";
                mgr._exitToEntrance[sourceKey] = sceneKey;
                mgr._entranceToExit[sceneKey] = sourceKey;

                mgr.PlayerData.SceneSpawnPosition = spawn.transform.position;
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    player.transform.position = spawn.transform.position;
                mgr._logger.Info($"Arrived at spawn '{label}' in '{scene.name}'");
                return;
            }
            Debug.LogError($"No Entrance with spawn '{label}' found in '{scene.name}'");
            return;
        }

        foreach (var entrance in FindObjectsByType<CanyonEntrance>(FindObjectsSortMode.None))
        {
            if (entrance.EntranceId != targetId) continue;
            CanyonSpawn spawn = entrance.GetSpawn(label);
            Vector3 pos = spawn != null ? spawn.transform.position : entrance.transform.position;
            mgr.PlayerData.SceneSpawnPosition = pos;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                player.transform.position = pos;
            mgr._logger.Info($"Arrived at spawn '{label}' in '{scene.name}'");
            return;
        }

        foreach (var exit in FindObjectsByType<CanyonExit>(FindObjectsSortMode.None))
        {
            if (exit.ExitId != targetId) continue;
            CanyonSpawn spawn = exit.GetSpawn(label);
            Vector3 pos = spawn != null ? spawn.transform.position : exit.transform.position;
            mgr.PlayerData.SceneSpawnPosition = pos;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                player.transform.position = pos;
            mgr._logger.Info($"Arrived at spawn '{label}' in '{scene.name}'");
            return;
        }
    }

    private void LogNetwork()
    {
        string path = string.Join(" → ", _visitedScenes);
        string current = _currentSceneIndex >= 0 && _currentSceneIndex < _visitedScenes.Count
            ? _visitedScenes[_currentSceneIndex].ToString()
            : "none";
        _logger.Info($"=== Canyon Network ===");
        _logger.Info($"Path: [{path}] (current: {current})");
        _logger.Info($"Links ({_exitToEntrance.Count}):");
        foreach (var kvp in _exitToEntrance)
            _logger.Info($"  {kvp.Key} ↔ {kvp.Value}");
    }

    private static SceneNames ParseSceneName(string name)
    {
        if (System.Enum.TryParse<SceneNames>(name, out var result))
            return result;
        return SceneNames.CanyonStart;
    }

    private static void Shuffle(List<SceneNames> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
