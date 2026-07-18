using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using ColePersistence;
using System.Collections.Generic;

// Note about instantiating objects here:
// World objects instantiated by this Manager should use Awake() instead of Start()
// Awake() is called when a prefab object is instantiated.
// Start() is called before first frame of scene, which has occured before instantiation.

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

    private void Awake() {
        _instance = this;
        _isFirstVisit = null;
        LoadScene();
    }

    private void OnDestroy() {
        _instance = null;
    }

    public void SaveScene() {
        SceneSaveData _sceneSaveData = new();
        _sceneSaveData.SaveDatas = GatherChildSaveData(ImpermanentContainer);
        _sceneSaveData.SceneExitGameTime = GameClock.Instance.GameMinutesElapsed;

        JsonPersistence.PersistJson<SceneSaveData>(_sceneSaveData, GetFileName()); 
    }

    private void LoadScene() {
        string _fileName = GetFileName();
        string _sceneName = SceneManager.GetActiveScene().name;

        // no save file
        if (!JsonPersistence.JsonExists(_fileName)) {
            _logger.Info($"{_sceneName} initial scene visit.");
            return;
        }

        // destroy defaults 
        DestroyChildren(ImpermanentContainer);

        // load from save
        var _loadedSaveData = JsonPersistence.FromJson<SceneSaveData>(_fileName);
        InstantiateAndLoadSavedObjects(_loadedSaveData.SaveDatas, ImpermanentContainer);
        ProcessElaspedTimeForChildren(_loadedSaveData.SceneExitGameTime, ImpermanentContainer);
        _logger.Info($"{_sceneName} loaded from save.");
    }

    // dang
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

        // get tickables
        foreach (Transform _child in parent)
            if (_child.TryGetComponent<GameClock.ITickable>(out var _tickable))
                _tickables.Add(_tickable);
        
        // tick tickables
        for (int i = 0; i < _elapsedGameMinutes; i++)
            foreach(var _tickable in _tickables)
                _tickable.OnGameMinuteTick();
    }

    private string GetFileName() {
        string _sceneName = SceneManager.GetActiveScene().name;
        return _sceneName + "_savedData.json";
    }
}