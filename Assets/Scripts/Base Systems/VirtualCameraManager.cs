using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VirtualCameraManager : MonoBehaviour
{
    [SerializeField] private PlayerData _playerData;
    CinemachineCamera _virtualCamera;

    private void Awake()
    {
        _virtualCamera = GetComponent<CinemachineCamera>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameMenu") return;
        GameObject _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null) return;

        _virtualCamera.OnTargetObjectWarped(_player.transform, _playerData.SceneSpawnPosition - transform.position);
    }
}
