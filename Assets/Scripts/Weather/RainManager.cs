using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum RainStates {Raining, NotRaining}; // clearsky?

public class RainManager : Singleton<RainManager> {
    private Transform _playerCamera;
    public event Action<RainStates> RainStateChange;
    private Transform _rainParticleSystem;
    private string _sceneName;
    private RainStates _rainState = RainStates.Raining;

    void Start() {
        _playerCamera = GameObject.FindWithTag("MainCamera").transform;
    }
    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    public void EnableRain() {
        _rainParticleSystem.gameObject.SetActive(true);
        _rainState = RainStates.Raining;
        RainStateChange.Invoke(_rainState);
    }
    public void DisableRain() {
        _rainParticleSystem.gameObject.SetActive(false);
        _rainState = RainStates.NotRaining;
        RainStateChange.Invoke(_rainState);
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        _sceneName = SceneManager.GetActiveScene().name;
        _rainParticleSystem = GameObject.FindGameObjectWithTag("Rain").transform;
    }

    void Update()
    {
        switch (_sceneName) { 
            case "Outside":
                _rainParticleSystem.position = _playerCamera.position;    
                break;
            case "Abandonded Shed":
                break;
        }
    }
}
