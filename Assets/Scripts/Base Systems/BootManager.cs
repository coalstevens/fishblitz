using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{
    [SerializeField] private bool _skipIntro = true;
    [SerializeField] private string _toScene;
    [SerializeField] private Vector3 _sceneSpawnLocation;
    void Awake()
    {
        GameStateManager.Initialize();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        ClearAllFilesInPersistentDataPath();
        StartCoroutine(OpeningDialogue());
    }

    IEnumerator OpeningDialogue() {
        if (_skipIntro != true) {
            yield return new WaitForSeconds(1f);
            NarratorSpeechController.Instance.PostMessage("You are wet.");
            NarratorSpeechController.Instance.PostMessage("You are freezing.");
            NarratorSpeechController.Instance.PostMessage("You are exhausted.");
            yield return new WaitForSeconds(11f); 
        }
        LoadInitialScene();
    }

    void LoadInitialScene() {
        LevelChanger.ChangeLevel(_toScene, _sceneSpawnLocation);
    }

    void OnSceneUnloaded(Scene current) {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void ClearAllFilesInPersistentDataPath()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath);

        foreach (string file in files)
            File.Delete(file);
    }
}
