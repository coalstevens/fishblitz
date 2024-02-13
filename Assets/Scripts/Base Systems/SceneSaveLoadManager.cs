using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Persistence;
using System;
using System.Linq;
using System.Reflection;

public abstract class WorldObjectSaveData {
        public string Identifier;
        public SimpleVector3 Position;

        public GameObject InstantiateGameObjectFromSave() {
            if (Identifier == null || Position == null) {
                Debug.LogError("There is no savedata to load the Worldobject");
                return null;
            }

            GameObject _prefab = Resources.Load<GameObject>("WorldObjects/" + Identifier);
            if (_prefab == null) {
                Debug.LogError($"Prefab not found for identifier: {Identifier}");
                return null;
            }

            Transform _worldObjectsContainer = GameObject.FindGameObjectWithTag("WorldObjectsContainer").transform;
            if (_worldObjectsContainer == null) { 
                Debug.LogError("There is no WorldObjectsContainer tagged in this scene.");
                return null;
            }

            Vector3 _savedPosition = new Vector3(Position.x, Position.y, Position.z);
            GameObject _newObject = UnityEngine.Object.Instantiate(_prefab, _savedPosition, Quaternion.identity, _worldObjectsContainer);
            Debug.Log("Instantiated a new " + Identifier);

            return _newObject;
        }
}

public class SceneSaveData {
    public List<WorldObjectSaveData> GameObjectSaves = new();
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
    Transform _worldObjectsContainer;
    private void Start() {
        _worldObjectsContainer = GameObject.FindGameObjectWithTag("WorldObjectsContainer").transform;
        LoadScene();
    }

    public void SaveScene() {
        Debug.Log("Saving Scene");
        SceneSaveData _sceneSaveData = new();
        IEnumerable<Type> _saveDataSubclasses = GetAllSubclasses<WorldObjectSaveData>();
        foreach (var _subclassType in _saveDataSubclasses) {
            dynamic instance = Activator.CreateInstance(_subclassType);
            MethodInfo gatherMethod = typeof(SceneSaveLoadManager).GetMethod("GatherWorldObjectSaveData").MakeGenericMethod(_subclassType);
            gatherMethod.Invoke(this, new object[] { instance });
        }
        _sceneSaveData.SceneExitGameTime = GameClock.GenerateCapture();
        

        JsonPersistence.PersistJson<SceneSaveData>(_sceneSaveData, GetFileName()); 
    }

    private async void LoadScene() {
        SceneSaveData _loadedSaveData;
        String _fileName = GetFileName();

        // no save file
        if (!JsonPersistence.JsonExists(_fileName)) {
            Debug.Log("No save file exists yet for scene: " + SceneManager.GetActiveScene().name);
            return;
        }

        // destroy defaults
        foreach (Transform _child in _worldObjectsContainer)
            Destroy(_child.gameObject);

        // load from save
        _loadedSaveData = await JsonPersistence.FromJson<SceneSaveData>(_fileName);
        LoadSavedWorldObjects(ref _loadedSaveData.GameObjectSaves);
        ProcessElapsedTime(_loadedSaveData.SceneExitGameTime);
    }
    
    private List<WorldObjectSaveData> GatherWorldObjectSaveData() {
        List<WorldObjectSaveData> gameObjectSaves = new();
        foreach (Transform _child in _worldObjectsContainer) {
            if (_child.TryGetComponent<ISaveable<WorldObjectSaveData>>(out var _saveable) ) {
                gameObjectSaves.Add(_saveable.Save());
                Debug.Log("Saving a: " + gameObjectSaves.Last().Identifier);
            }
        }
        return gameObjectSaves;
    }
    private List<T> GatherWorldObjectSaveData<T>() where T : WorldObjectSaveData {
        List<T> gameObjectSaves = new();
        foreach (Transform _child in _worldObjectsContainer) {
            if (_child.TryGetComponent<ISaveable<T>>(out var _saveable) ) {
                gameObjectSaves.Add(_saveable.Save());
                Debug.Log("Saving a: " + gameObjectSaves.Last().Identifier);
            }
        }
        return gameObjectSaves;
    }

    private void LoadSavedWorldObjects (ref List<WorldObjectSaveData> gameObjectSaves) {
        foreach (var _save in gameObjectSaves) {
            var newObject = _save.InstantiateGameObjectFromSave().GetComponent<ISaveable<WorldObjectSaveData>>();
            Debug.Log("Loading a: " + _save.Identifier);
            newObject.Load(_save);
        }
    }

    private void ProcessElapsedTime(GameClockCapture pastTime) {  
        int _elapsedGameMinutes = GameClock.CalculateElapsedGameMinutesSinceTime(pastTime);
        Debug.Log("Processing " + _elapsedGameMinutes + " game minutes.");
        List<ITickable> _tickables = new();

        // get tickables
        foreach (Transform _child in _worldObjectsContainer)
            if (_child.TryGetComponent<ITickable>(out var _tickable))
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

    private IEnumerable<Type> GetAllSubclasses<TBase>() {
        Type baseType = typeof(TBase);
        return Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(baseType));
    }
}

