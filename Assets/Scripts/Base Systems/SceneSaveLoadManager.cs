using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ColePersistence;
using System.Collections.Generic;

// Note about instantiating objects here:
// World objects instantiated by this Manager should use Awake() instead of Start()
// Awake() is called when a prefab object is instantiated.
// Start() is called before first frame of scene, which has occured before instantiation.

public class SceneSaveLoadManager : MonoBehaviour {
    public interface ISaveable 
    {
        SaveData Save();
        void Load(SaveData saveData);
    }
    [SerializeField] private Logger _logger = new();

    Transform _impermanentContainer;
    public delegate void FirstVisitToSceneHandler(string sceneName);
    public static event FirstVisitToSceneHandler FirstVisitToScene;
    private class SceneSaveData {
        public List<SaveData> SaveDatas = new();
        public int SceneExitGameTime;
    }

    private void Awake() {
        _impermanentContainer = GameObject.FindGameObjectWithTag("Impermanent").transform;
        LoadScene();
    }

    public void SaveScene() {
        SceneSaveData _sceneSaveData = new();
        _sceneSaveData.SaveDatas = GatherChildSaveData(_impermanentContainer);
        _sceneSaveData.SceneExitGameTime = GameClock.Instance.GameMinutesElapsed;

        JsonPersistence.PersistJson<SceneSaveData>(_sceneSaveData, GetFileName()); 
    }

    private void LoadScene() {
        string _fileName = GetFileName();
        string _sceneName = SceneManager.GetActiveScene().name;

        // no save file
        if (!JsonPersistence.JsonExists(_fileName)) {
            _logger.Info($"{_sceneName} initial scene visit.");
            StartCoroutine(InvokeFirstVisitToSceneAfterFrame(_sceneName));
            return;
        }

        // destroy defaults 
        DestroyChildren(_impermanentContainer);

        // load from save
        var _loadedSaveData = JsonPersistence.FromJson<SceneSaveData>(_fileName);
        InstantiateAndLoadSavedObjects(_loadedSaveData.SaveDatas, _impermanentContainer);
        ProcessElaspedTimeForChildren(_loadedSaveData.SceneExitGameTime, _impermanentContainer);
        _logger.Info($"{_sceneName} loaded from save.");
    }

    // hack so that the additive scenes (narrator, hud, etc) have time to load in
    private System.Collections.IEnumerator InvokeFirstVisitToSceneAfterFrame(string sceneName)
    {
        yield return null; // wait for the next frame
        FirstVisitToScene?.Invoke(sceneName);
    }

    // dang
    private void DestroyChildren(Transform parent) {
        foreach (Transform _child in parent)
            Destroy(_child.gameObject);
    }

    private List<SaveData> GatherChildSaveData(Transform parent) {
        List<SaveData> _saveDatas = new();
        foreach (Transform _child in parent)
            if (_child.TryGetComponent<ISaveable>(out var _saveable) ) {
                SaveData _savedata = _saveable.Save();
                _saveDatas.Add(_savedata);
            }
        return _saveDatas; 
    }

    private void InstantiateAndLoadSavedObjects(List<SaveData> saveDatas, Transform container) {
        foreach (var _saveData in saveDatas) {
            var newObject = _saveData.InstantiateGameObjectFromSaveData(container).GetComponent<ISaveable>();
            newObject.Load(_saveData);
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