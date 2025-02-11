using System.Runtime.CompilerServices;
using OysterUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour, PlayerInteractionManager.IInteractable
{
    [SerializeField] bool OnInteract = false;
    [SerializeField] private string _toScene;
    [SerializeField] private Vector3 _spawnLocation;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private AudioClip _sound;
    [SerializeField] private float _soundVolume = 1;
    private Scene test;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!OnInteract && other == GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>())
        {
            _playerData.SceneSpawnPosition = _spawnLocation;
            PlaySound();
            ChangeLevel(_toScene);
        }
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (OnInteract)
        {
            _playerData.SceneSpawnPosition = _spawnLocation;
            PlaySound();
            ChangeLevel(_toScene);
            return true;
        }
        return false;
    }

    public static void ChangeLevel(string sceneName)
    {
        SmoothSceneManager.LoadScene(sceneName);
    }

    private void PlaySound()
    {
        if (_sound != null)
            AudioManager.Instance.PlaySFX(_sound, _soundVolume);
    }

}
