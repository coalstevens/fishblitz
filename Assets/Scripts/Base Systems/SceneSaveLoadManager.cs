using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using ColePersistence;
using System.Collections.Generic;

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
    public static bool IsFirstVisit {
        get {
            if (_instance == null) {
                Debug.LogError("No SceneSaveLoadManager in scene — cannot determine IsFirstVisit.");
                return false;
            }
            if (_isFirstVisit == null) {
                string sceneName = SceneManager.GetActiveScene().name;
                string fileName = sceneName + "_savedData.json";
                _isFirstVisit = !JsonPersistence.JsonExists(fileName);
            }
            return _isFirstVisit.Value;
        }
    }

    private class SceneSaveData {
        public List<SaveData> SaveDatas = new();
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
    }

    private void Start() {
        LoadPlayerComponents();
    }

    private void OnApplicationQuit() {
        SavePlayerComponents();
    }

    private void OnDestroy() {
        if (_instance == this)
            _instance = null;
    }

    // --- World Object Save/Load (per-scene) ---

    public void SaveScene() {
        SceneSaveData _sceneSaveData = new();
        _sceneSaveData.SaveDatas = GatherChildSaveData(ImpermanentContainer);
        _sceneSaveData.SceneExitGameTime = GameClock.Instance.GameMinutesElapsed;

        JsonPersistence.PersistJson<SceneSaveData>(_sceneSaveData, GetSceneFileName()); 
    }

    private void LoadScene() {
        string _fileName = GetSceneFileName();
        string _sceneName = SceneManager.GetActiveScene().name;

        if (!JsonPersistence.JsonExists(_fileName)) {
            _logger.Info($"{_sceneName} initial scene visit.");
            return;
        }

        DestroyChildren(ImpermanentContainer);

        var _loadedSaveData = JsonPersistence.FromJson<SceneSaveData>(_fileName);
        InstantiateAndLoadSavedObjects(_loadedSaveData.SaveDatas, ImpermanentContainer);
        ProcessElaspedTimeForChildren(_loadedSaveData.SceneExitGameTime, ImpermanentContainer);
        _logger.Info($"{_sceneName} loaded from save.");
    }

    // --- Player Component Save/Load (global, persists across scenes) ---

    public static void SavePlayerComponents() {
        var _saveableComponents = FindAllSaveableComponents();
        if (_saveableComponents.Count == 0) return;

        var _saveData = new PlayerComponentsSaveData();
        foreach (var _component in _saveableComponents) {
            try {
                var _json = _component.CaptureStateAsJson();
                if (!string.IsNullOrEmpty(_json))
                    _saveData.ComponentStates[_component.ComponentId] = _json;
            }
            catch (System.Exception ex) {
                Debug.LogError($"Failed to save component '{_component.ComponentId}': {ex.Message}");
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

        var _saveableComponents = FindAllSaveableComponents();
        foreach (var _component in _saveableComponents) {
            if (_saveData.ComponentStates.TryGetValue(_component.ComponentId, out var _json)) {
                try {
                    _component.RestoreStateFromJson(_json);
                }
                catch (System.Exception ex) {
                    Debug.LogWarning($"Failed to restore component '{_component.ComponentId}': {ex.Message}");
                }
            }
        }

        _logger.Info("Player components loaded from save.");
    }

    private static List<ISaveableComponent> FindAllSaveableComponents() {
        var _results = new List<ISaveableComponent>();
        var _allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var _mb in _allMonoBehaviours) {
            if (_mb is ISaveableComponent _saveable)
                _results.Add(_saveable);
        }
        return _results;
    }

    // --- Shared utilities ---

    private void DestroyChildren(Transform parent) {
        foreach (Transform _child in parent)
            Destroy(_child.gameObject);
    }

    private List<SaveData> GatherChildSaveData(Transform parent) {
        List<SaveData> _saveDatas = new();
        foreach (Transform _child in parent) {
            SaveData _saveData;
            if (_child.TryGetComponent<SaveData.ISaveable>(out var _saveable)) {
                _saveData = _saveable.Save();
            } else {
                _saveData = new SaveData();
                _saveData.AddIdentifier(_child.gameObject.name.Replace("(Clone)", ""));
                _saveData.AddTransformPosition(_child.position);
            }
            _saveDatas.Add(_saveData);
        }
        return _saveDatas;
    }

    private void InstantiateAndLoadSavedObjects(List<SaveData> saveDatas, Transform container) {
        foreach (var _saveData in saveDatas) {
            var newObject = _saveData.InstantiateGameObjectFromSaveData(container);
            if (newObject.TryGetComponent<SaveData.ISaveable>(out var _saveable))
                _saveable.Load(_saveData);
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
        return _sceneName + "_savedData.json";
    }
}
