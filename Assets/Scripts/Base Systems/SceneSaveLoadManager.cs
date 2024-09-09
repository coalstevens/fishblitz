using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ColePersistence;
using System;

// Note about instantiating objects here:
// World objects instantiated by this Manager should use Awake() instead of Start()
// Start() is called before first frame of scene, which has already passed.
// Awake() is called when a prefab object is instantiated.
public class SceneSaveLoadManager : MonoBehaviour {
    Transform _impermanentContainer;
    private class SceneSaveData {
        public List<SaveData> SaveDatas = new();
        public GameClockCapture SceneExitGameTime;
    }

    private void Start() {
        _impermanentContainer = GameObject.FindGameObjectWithTag("Impermanent").transform;
        LoadScene();
    }

    public async void SaveScene() {
        SceneSaveData _sceneSaveData = new();
        _sceneSaveData.SaveDatas = GatherSaveDataInParent(_impermanentContainer);
        _sceneSaveData.SceneExitGameTime = GameClock.GenerateCapture();

        // Save
        await JsonPersistence.PersistJson<SceneSaveData>(_sceneSaveData, GetFileName()); 
    }

    private async void LoadScene() {
        String _fileName = GetFileName();

        // no save file
        if (!JsonPersistence.JsonExists(_fileName)) {
            string sceneName = SceneManager.GetActiveScene().name;
            // Debug.Log("No save file exists yet for scene: " + sceneName);
            PrintFirstTimeSceneVisitedMessage(sceneName);
            return;
        }

        // destroy defaults
        DestroyChildren(_impermanentContainer);

        // load from save
        var _loadedSaveData = await JsonPersistence.FromJson<SceneSaveData>(_fileName);
        LoadSaveDataIntoParent(_loadedSaveData.SaveDatas, _impermanentContainer);
        ProcessElapsedTimeInParent(_loadedSaveData.SceneExitGameTime, _impermanentContainer);
    }

    private void PrintFirstTimeSceneVisitedMessage(string sceneName) 
    {
        switch(sceneName) {
            case "Outside": 
                NarratorSpeechController.Instance.PostMessage("Press 'v' to interact. Press space to use a tool.");
                break;
            case "Abandoned Shed":
                break;
            default:
                break;
        }
    }
    
    private void DestroyChildren(Transform parent) {
        foreach (Transform _child in parent)
            Destroy(_child.gameObject);
    }

    private List<SaveData> GatherSaveDataInParent(Transform parent) {
        List<SaveData> _saveDatas = new();
        foreach (Transform _child in parent)
            if (_child.TryGetComponent<ISaveable>(out var _saveable) ) {
                SaveData _savedata = _saveable.Save();
                _saveDatas.Add(_savedata);
            }
        return _saveDatas; 
    }

    private void LoadSaveDataIntoParent(List<SaveData> saveDatas, Transform parent) {
        foreach (var _saveData in saveDatas) {
            var newObject = _saveData.InstantiateGameObjectFromSaveData(parent).GetComponent<ISaveable>();
            newObject.Load(_saveData);
        }
    }

    private void ProcessElapsedTimeInParent(GameClockCapture pastTime, Transform parent) {  
        int _elapsedGameMinutes = GameClock.CalculateElapsedGameMinutesSinceTime(pastTime);
        // Debug.Log("Processing " + _elapsedGameMinutes + " game minutes.");
        List<ITickable> _tickables = new();

        // get tickables
        foreach (Transform _child in parent)
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
}