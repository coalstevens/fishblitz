using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VirtualCameraManager : MonoBehaviour
{
    CinemachineVirtualCamera _virtualCamera;
    private void Awake() {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }
    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "GameMenu") return;
        Transform _player = GameObject.FindGameObjectWithTag("Player").transform;
        _virtualCamera.OnTargetObjectWarped(_player, PlayerData.Instance.SceneSpawnPosition - transform.position);
    }
}
