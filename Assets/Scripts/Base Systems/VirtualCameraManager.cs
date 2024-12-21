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
        GameObject _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null) return;

        _virtualCamera.OnTargetObjectWarped(_player.transform, PlayerData.SceneSpawnPosition - transform.position);
    }
}
