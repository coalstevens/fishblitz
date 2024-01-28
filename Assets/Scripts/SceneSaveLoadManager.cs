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

public class SceneSaveLoadManager : MonoBehaviour
{
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

    private void LoadSaveFile() {
        if (JsonPersistence.JsonExists(_fileName)) {
            foreach (Transform _child in _worldObjectsContainer) {
                Destroy(_child.gameObject);
            }
            LoadWorldObjects();
        }
    }

    public void Save() {
        SaveWorldObjects();
    }

    private void SaveWorldObjects()
    {
        List<WorldObjectSaveData> _worldObjectsToSave = new List<WorldObjectSaveData>();
        foreach (Transform _child in _worldObjectsContainer) { 
            if (_child.TryGetComponent<IWorldObject>(out IWorldObject _worldObject)) {
                var _objectToSave = new WorldObjectSaveData {
                    Identifier = _worldObject.Identifier,
                    State = _worldObject.State,
                    Position = new SimpleVector3(_child.gameObject.transform.position)
                };

                if (_child.TryGetComponent<IItemStorage>(out IItemStorage _itemStorer)) {
                    _objectToSave.ItemQuantitys = _itemStorer.ItemQuantities;
                }

                _worldObjectsToSave.Add(_objectToSave);
            }
        }
        JsonPersistence.PersistJson<List<WorldObjectSaveData>>(_worldObjectsToSave, _fileName);
    }

    private async void LoadWorldObjects() {
        var _loadedObjects = await JsonPersistence.FromJson<List<WorldObjectSaveData>>(_fileName);
        
        foreach (var _loadedObject in _loadedObjects) {
            GameObject _prefab = Resources.Load<GameObject>("WorldObjects/" + _loadedObject.Identifier);

            if (_prefab != null) {
                Vector3 _position = new Vector3(_loadedObject.Position.x, _loadedObject.Position.y, _loadedObject.Position.y);
                GameObject _newObject = Instantiate(_prefab, _position, Quaternion.identity, _worldObjectsContainer);
                IWorldObject _worldObject = _newObject.GetComponent<IWorldObject>();
                _worldObject.State = _loadedObject.State;

                if (_worldObject is IItemStorage _itemStorageComponent && _loadedObject.ItemQuantitys != null) {
                    _itemStorageComponent.ItemQuantities =_loadedObject.ItemQuantitys;
                }
            }
            else
            {
                Debug.LogError($"Prefab not found for identifier: {_loadedObject.Identifier}");
            }
        }
    }
}

