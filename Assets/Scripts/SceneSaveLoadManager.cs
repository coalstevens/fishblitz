using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Persistence;

public class WorldObjectSaveData
{
    public string Identifier;
    public int State;
    public SimpleVector3 Position;
    public Dictionary<string, int> ItemQuantitys;
    public List<float> CountersGameMinutes;
}

public class SceneSaveData {
    public List<WorldObjectSaveData> WorldObjects;
    public GameClockCapture SceneExitGameTime;
}

public class SimpleVector3 {
    public float x;
    public float y;
    public float z;
    
    public SimpleVector3(Vector3 _vector) {
        x = _vector.x;
        y = _vector.y;
        z = _vector.z;
    }
}

public class SceneSaveLoadManager : MonoBehaviour {
    private Transform _worldObjectsContainer;
    private string _fileName;

    void Start() {
        _worldObjectsContainer = GameObject.FindGameObjectWithTag("WorldObjectsContainer").transform;
        _fileName = GetFileName();
        LoadSaveFile();
    }

    private string GetFileName() {
        Scene currentScene = SceneManager.GetActiveScene();
        string _sceneName = currentScene.name;
        return _sceneName + "_savedData.json";
    }

    private async void LoadSaveFile() {
        SceneSaveData _loadedSaveData;
        if (JsonPersistence.JsonExists(_fileName)) {
            _loadedSaveData = await JsonPersistence.FromJson<SceneSaveData>(_fileName);
            LoadWorldObjects(_loadedSaveData);
        }
    }

    public void Save() {
        SceneSaveData _saveData = new();
        _saveData.WorldObjects = SaveWorldObjects();
        _saveData.SceneExitGameTime = GameClock.GenerateCapture();

        JsonPersistence.PersistJson<SceneSaveData>(_saveData, _fileName); 
    }

    private List<WorldObjectSaveData> SaveWorldObjects()
    {
        List<WorldObjectSaveData> _worldObjectsToSave = new List<WorldObjectSaveData>();
        foreach (Transform _child in _worldObjectsContainer) { 
            if (_child.TryGetComponent<IWorldObject>(out IWorldObject _worldObject)) {
                Debug.Log("Saved a " + _worldObject.Identifier);
                var _objectToSave = new WorldObjectSaveData {
                    Identifier = _worldObject.Identifier,
                    State = _worldObject.State,
                    Position = new SimpleVector3(_child.gameObject.transform.position)
                };

                if (_child.TryGetComponent<IItemStorage>(out var _itemStorer))
                    _objectToSave.ItemQuantitys = _itemStorer.ItemQuantities;

                if (_child.TryGetComponent<ITimeSensitive>(out var _timeSensitive))
                    _objectToSave.CountersGameMinutes = _timeSensitive.CountersGameMinutes;

                _worldObjectsToSave.Add(_objectToSave);
            }
        }
        return _worldObjectsToSave;
    }

    private void LoadWorldObjects(SceneSaveData loadedSaveData) {
        // Destroy default objects in scene 
        foreach (Transform _child in _worldObjectsContainer) {
                Destroy(_child.gameObject);
        }
        
        // Load saved objects
        var _loadedObjects = loadedSaveData.WorldObjects;
        List<ITimeSensitive> _timeSensitives = new();
        
        foreach (var _loadedObject in _loadedObjects) {
            GameObject _prefab = Resources.Load<GameObject>("WorldObjects/" + _loadedObject.Identifier);
            if (_prefab == null) {
                Debug.LogError($"Prefab not found for identifier: {_loadedObject.Identifier}");
                continue;
            }

            Vector3 _savedPosition = new Vector3(_loadedObject.Position.x, _loadedObject.Position.y, _loadedObject.Position.y);
            GameObject _newObject = Instantiate(_prefab, _savedPosition, Quaternion.identity, _worldObjectsContainer);
            IWorldObject _worldObject = _newObject.GetComponent<IWorldObject>();
            _worldObject.State = _loadedObject.State;

            if (_worldObject is IItemStorage _itemStorageComponent && _loadedObject.ItemQuantitys != null)
                _itemStorageComponent.ItemQuantities =_loadedObject.ItemQuantitys;  
            if (_worldObject is ITimeSensitive _timeSensitive) {
                _timeSensitive.CountersGameMinutes = _loadedObject.CountersGameMinutes;
                _timeSensitives.Add(_timeSensitive);
            }
            Debug.Log("Loaded a " + _worldObject.Identifier);
        }

        ProcessElapsedTime(_timeSensitives, loadedSaveData.SceneExitGameTime);
    }

    private void ProcessElapsedTime(List<ITimeSensitive> worldObjectsToProcess, GameClockCapture pastTime) {
        int _elapsedGameMinutes = GameClock.CalculateElapsedGameMinutesSinceTime(pastTime);
        List<ITimeSensitive> _timeSensitives = new(); 
        List<ITimeSensitive> _heatSources = new();
        List<ITimeSensitive> _heatSensitives =new();

        foreach (var _worldObject in worldObjectsToProcess) {
            if (_worldObject is IHeatSource)
                _heatSources.Add(_worldObject);
            else if (_worldObject is IHeatSensitive)
                _heatSensitives.Add(_worldObject);
            else
                _timeSensitives.Add(_worldObject);
        }

        for (int i = 0; i <_elapsedGameMinutes; i++) {
            foreach (var _heatSource in _heatSources)
                _heatSource.OnGameMinuteTick();
            foreach (var _heatSensitive in _heatSensitives)
                _heatSensitive.OnGameMinuteTick();
            foreach (var _worldObject in _timeSensitives)
                _worldObject.OnGameMinuteTick();
        }
    }
}

