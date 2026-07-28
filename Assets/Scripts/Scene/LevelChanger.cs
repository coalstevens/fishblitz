using OysterUtils;
using UnityEngine;

public class LevelChanger : MonoBehaviour, InteractInput.IInteractable
{
    [SerializeField] bool OnInteract = false;
    [SerializeField] private string _toScene;
    [SerializeField] private Vector3 _spawnLocation;
    [SerializeField] private SoundData _sound;
    [SerializeField] private AudioSource _audioSource;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Player seen");
        if (!OnInteract && other.transform.root.CompareTag("Player"))
        {
            Debug.Log("Player confirmed");
            PlayerSceneData.PendingSpawnPosition = _spawnLocation;
            PlayerSceneData.HasPendingSpawn = true;
            PlaySound();
            ChangeLevel(_toScene);
        }
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (OnInteract)
        {
            PlayerSceneData.PendingSpawnPosition = _spawnLocation;
            PlayerSceneData.HasPendingSpawn = true;
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
