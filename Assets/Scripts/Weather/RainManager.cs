using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum RainStates {Raining, NotRaining}; // clearsky?

public class RainManager : Singleton<RainManager> {
    private Transform _playerCamera;
    private RainStates _rainState = RainStates.Raining; // SUBSCRIBE WITH EVENTS
    public event Action<RainStates> RainStateChange;
    private GameObject _rainParticleSystem;
    private const float RAIN_Y_OFFSET = 6f;
    private string _sceneName;

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
        _rainParticleSystem = GameObject.FindGameObjectWithTag("Rain");
    }

    void Update()
    {
        switch (_sceneName) { 
            case "Outside":
                //_rainParticleSystem.transform.position = _playerCamera.position;
                Vector3 _newPosition = _playerCamera.position;
                _newPosition.y += RAIN_Y_OFFSET;
                _rainParticleSystem.transform.position = _newPosition; 
                break;
            case "Abandonded Shed":
                break;
            case "Boot":
                break;
            default:
                break;
        }
    }
}
