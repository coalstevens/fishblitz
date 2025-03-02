using System;
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
        ClearAllFilesInPersistentDataPath();
        StartCoroutine(OpeningDialogue());
    }
    private void Start()
    {
        WorldStateByCalendar.UpdateWorldState();
    }

    private IEnumerator OpeningDialogue() {
        if (!_skipIntro) {
            yield return new WaitForSeconds(1f);
            NarratorSpeechController.Instance.PostMessage("...");
            NarratorSpeechController.Instance.PostMessage("everything feels heavy...");
            yield return new WaitUntil(() => NarratorSpeechController.Instance.AreMessagesClear());
            yield return new WaitForSeconds(1f); 
        }
        LoadInitialScene();
    }

    private void LoadInitialScene() {
        _playerData.SceneSpawnPosition = _sceneSpawnLocation;
        LevelChanger.ChangeLevel(_toScene);
    }

    private void ClearAllFilesInPersistentDataPath()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath);

        foreach (string file in files)
            File.Delete(file);
    }
}
