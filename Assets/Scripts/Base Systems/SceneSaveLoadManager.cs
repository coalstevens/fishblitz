using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using ColePersistence;
using System.Collections.Generic;
using System.IO;

// Note about instantiating objects here:
// World objects instantiated by this Manager should use Awake() instead of Start()
// Awake() is called when a prefab object is instantiated.
// Start() is called before first frame of scene, which has occured before instantiation.

[DefaultExecutionOrder(100)]
public class SceneSaveLoadManager : MonoBehaviour {

    [SerializeField] private Logger _logger = new();

    private Transform _impermanentContainer;
    private Transform ImpermanentContainer {
        get {
            if (_impermanentContainer == null) {
                _impermanentContainer = GameObject.FindGameObjectWithTag("Impermanent").transform;
                if (!_impermanentContainer.TryGetComponent<WorldObjectOccupancyMap>(out _))
                    Debug.LogError("Impermanent container is missing WorldObjectOccupancyMap.");
                if (!_impermanentContainer.TryGetComponent<Tilemap>(out _))
                    Debug.LogError("Impermanent container is missing Tilemap.");
                if (!_impermanentContainer.TryGetComponent<TilemapRenderer>(out _))
                    Debug.LogError("Impermanent container is missing TilemapRenderer.");
            }
            return _impermanentContainer;
        }
    }

    private static SceneSaveLoadManager _instance;
    private static bool? _isFirstVisit = null;
    public static string SceneSaveSuffix { get; set; } = "";

    public static bool IsFirstVisit {
        get {
            if (_instance == null) {
                Debug.LogError("No SceneSaveLoadManager in scene — cannot determine IsFirstVisit.");
                return false;
            }
            if (_isFirstVisit == null) {
                _isFirstVisit = !JsonPersistence.JsonExists(_instance.GetSceneFileName());
            }
            return _isFirstVisit.Value;
        }
    }

    private class SceneSaveData {
        public List<SceneObjectRecord> Records = new();
        public int SceneExitGameTime;
    }

    private class PlayerComponentsSaveData {
        public Dictionary<string, string> ComponentStates = new();
    }

    private const string PLAYER_SAVE_FILE = "PlayerComponents.json";

    private void Awake() {
        _instance = this;
        _isFirstVisit = null;
        LoadScene();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        LoadPlayerComponents();
        _impermanentContainer = null;
        _isFirstVisit = null;
        LoadScene();
    }

    private void Start() 
    {
        LoadPlayerComponents();
    }

    private void OnApplicationQuit() {
        SavePlayerComponents();
    }

    // --- World Object Save/Load (per-scene) ---

    public void SaveScene() {
        SceneSaveData _sceneSaveData = new();
        _sceneSaveData.Records = GatherChildRecords(ImpermanentContainer);
        _sceneSaveData.SceneExitGameTime = GameClock.Instance.GameMinutesElapsed;

        JsonPersistence.PersistJson(_sceneSaveData, GetSceneFileName()); 
    }

    private void LoadScene() {
        string _fileName = GetSceneFileName();
        string _sceneName = SceneManager.GetActiveScene().name;

        if (!JsonPersistence.JsonExists(_fileName)) {
            _logger.Info($"{_sceneName} initial scene visit.");
            return;
        }

        var _loadedSaveData = JsonPersistence.FromJson<SceneSaveData>(_fileName);
        if (_loadedSaveData?.Records == null) {
            _logger.Warning($"{_sceneName} save is in an old or invalid format; treating as first visit.");
            return;
        }

        DestroyChildren(ImpermanentContainer);

        InstantiateAndLoadSavedObjects(_loadedSaveData.Records, ImpermanentContainer);
        ProcessElaspedTimeForChildren(_loadedSaveData.SceneExitGameTime, ImpermanentContainer);
        _logger.Info($"{_sceneName} loaded from save.");
    }

    // --- Player Component Save/Load (global, persists across scenes) ---

    public static void SavePlayerComponents() {
        var _saveables = FindAllSaveables();
        if (_saveables.Count == 0) return;

        var _saveData = new PlayerComponentsSaveData();
        foreach (var _saveable in _saveables) {
            try {
                var _json = _saveable.CaptureState();
                if (!string.IsNullOrEmpty(_json))
                    _saveData.ComponentStates[_saveable.SaveableId] = _json;
            }
            catch (System.Exception ex) {
                Debug.LogError($"Failed to save component '{_saveable.SaveableId}': {ex.Message}");
            }
        }

        JsonPersistence.PersistJson(_saveData, PLAYER_SAVE_FILE);
    }

    private void LoadPlayerComponents() {
        if (!JsonPersistence.JsonExists(PLAYER_SAVE_FILE)) {
            _logger.Info("No player component save found.");
            return;
        }

        var _saveData = JsonPersistence.FromJson<PlayerComponentsSaveData>(PLAYER_SAVE_FILE);
        if (_saveData?.ComponentStates == null) return;

        var _saveables = FindAllSaveables();
        foreach (var _saveable in _saveables) {
            if (_saveData.ComponentStates.TryGetValue(_saveable.SaveableId, out var _json)) {
                try {
                    _saveable.RestoreState(_json);
                }
                catch (System.Exception ex) {
                    Debug.LogWarning($"Failed to restore component '{_saveable.SaveableId}': {ex.Message}");
                }
            }
        }

        _logger.Info("Player components loaded from save.");
    }

    // World objects are ISceneSaveable, not player components — they persist per-scene,
    // so they must not be captured into the global PlayerComponents file.
    private static List<ISaveable> FindAllSaveables() {
        var _results = new List<ISaveable>();
        var _allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var _mb in _allMonoBehaviours) {
            if (_mb is ISceneSaveable) continue;
            if (_mb is ISaveable _saveable)
                _results.Add(_saveable);
        }
        return _results;
    }

    // --- Shared utilities ---

    private void DestroyChildren(Transform parent) {
        foreach (Transform _child in parent)
            Destroy(_child.gameObject);
    }

    private List<SceneObjectRecord> GatherChildRecords(Transform parent) {
        List<SceneObjectRecord> _records = new();
        foreach (Transform _child in parent) {
            SceneObjectRecord _record;
            if (_child.TryGetComponent<ISceneSaveable>(out var _saveable)) {
                _record = SceneObjectRecord.Capture(_saveable, _child.position);
            } else {
                _record = new SceneObjectRecord {
                    PrefabId = _child.gameObject.name.Replace("(Clone)", ""),
                    Position = _child.position
                };
            }
            _records.Add(_record);
        }
        return _records;
    }

    private void InstantiateAndLoadSavedObjects(List<SceneObjectRecord> records, Transform container) {
        foreach (var _record in records) {
            var newObject = _record.Instantiate(container);
            if (newObject == null) continue;
            if (newObject.TryGetComponent<ISceneSaveable>(out var _saveable))
                _record.Restore(_saveable);
        }
    }

    private void ProcessElaspedTimeForChildren(int pastTime, Transform parent) {  
        int _elapsedGameMinutes = GameClock.CalculateElapsedGameMinutesSinceTime(pastTime);
        List<GameClock.ITickable> _tickables = new();

        foreach (Transform _child in parent)
            if (_child.TryGetComponent<GameClock.ITickable>(out var _tickable))
                _tickables.Add(_tickable);
        
        for (int i = 0; i < _elapsedGameMinutes; i++)
            foreach(var _tickable in _tickables)
                _tickable.OnGameMinuteTick();
    }

    private string GetSceneFileName() {
        string _sceneName = SceneManager.GetActiveScene().name;
        string suffix = string.IsNullOrEmpty(SceneSaveSuffix) ? "" : "_" + SceneSaveSuffix;
        return _sceneName + suffix + "_savedData.json";
    }

    public static void ClearTWNSaves(HashSet<string> twnSceneNames) {
        string[] files = Directory.GetFiles(Application.persistentDataPath, "*_savedData.json");
        foreach (string file in files) {
            string fileName = Path.GetFileName(file);
            string baseName = fileName.Substring(0, fileName.Length - "_savedData.json".Length);
            int lastUnderscore = baseName.LastIndexOf('_');
            if (lastUnderscore < 0) continue;
            string sceneName = baseName.Substring(0, lastUnderscore);
            string suffix = baseName.Substring(lastUnderscore + 1);
            if (int.TryParse(suffix, out _) && twnSceneNames.Contains(sceneName))
                JsonPersistence.DeleteFile(fileName);
        }
    }
}
