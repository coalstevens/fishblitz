using System.Runtime.CompilerServices;
using OysterUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour, InteractInput.IInteractable
{
    [SerializeField] bool OnInteract = false;
    [SerializeField] private string _toScene;
    [SerializeField] private Vector3 _spawnLocation;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private SoundData _sound;
    [SerializeField] private AudioSource _audioSource;
    private Scene test;
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Player seen");
        if (!OnInteract && other.transform.root.CompareTag("Player"))
        {
            Debug.Log("Player confirmed");
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

    public static void ChangeLevel(SceneNames scene)
    {
        ChangeLevel(scene.ToString());
    }

    private void PlaySound()
    {
        if (_sound != null)
            AudioManager.PlaySFX(_audioSource, _sound);
    }

}
