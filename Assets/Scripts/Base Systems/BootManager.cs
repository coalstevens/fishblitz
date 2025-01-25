using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{
    [SerializeField] private bool _skipIntro = true;
    [SerializeField] private string _toScene;
    [SerializeField] private Vector3 _sceneSpawnLocation;
    [SerializeField] private PlayerData _playerData;
    private void Awake()
    {
        GameStateManager.Initialize();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        ClearAllFilesInPersistentDataPath();
        StartCoroutine(OpeningDialogue());
    }

    private IEnumerator OpeningDialogue() {
        if (_skipIntro != true) {
            yield return new WaitForSeconds(1f);
            NarratorSpeechController.Instance.PostMessage("You are wet.");
            NarratorSpeechController.Instance.PostMessage("You are freezing.");
            NarratorSpeechController.Instance.PostMessage("You are exhausted.");
            yield return new WaitForSeconds(11f); 
        }
        LoadInitialScene();
    }

    private void LoadInitialScene() {
        _playerData.SceneSpawnPosition = _sceneSpawnLocation;
        LevelChanger.ChangeLevel(_toScene);
    }

    private void OnSceneUnloaded(Scene current) {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void ClearAllFilesInPersistentDataPath()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath);

        foreach (string file in files)
            File.Delete(file);
    }
}
