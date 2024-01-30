using System.Collections;
using System.Collections.Generic;
using System.IO;
using OysterUtils;
using UnityEngine;

public class BootManager : MonoBehaviour
{
    [SerializeField] private string _toScene;
    [SerializeField] private Vector3 _sceneSpawnLocation;
    void Start()
    {
        SmoothSceneManager.LoadScene(_toScene);
        PlayerData.Instance.SceneSpawnPosition = _sceneSpawnLocation; 
        ClearAllFilesInPersistentDataPath();
    }

    void ClearAllFilesInPersistentDataPath()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath);

        foreach (string file in files)
        {
            File.Delete(file);
        }

        Debug.Log("All files in persistent data path deleted.");
    }
}
