using System.Collections;
using System.Collections.Generic;
using System.IO;
using OysterUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{
    [SerializeField] private bool _skipIntro = true;
    [SerializeField] private string _toScene;
    [SerializeField] private Vector3 _sceneSpawnLocation;
    private SpriteRenderer _player;
    private Transform _activeGridCell;
    private Transform _itemCursor;
    private Transform _inventoryContainer;
    private Transform _topRightHUD;
    void Start()
    {
        // Events
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // References
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>();
        _activeGridCell = GameObject.FindGameObjectWithTag("ActiveGridCell").transform;
        _itemCursor = GameObject.FindGameObjectWithTag("ItemCursor").transform;
        _inventoryContainer = GameObject.FindGameObjectWithTag("InventoryContainer").transform;
        _topRightHUD = GameObject.FindGameObjectWithTag("TopRightHUD").transform;

        // Disable visual elements
        _player.enabled = false;
        _activeGridCell.gameObject.SetActive(false);
        _itemCursor.gameObject.SetActive(false);
        _inventoryContainer.gameObject.SetActive(false);
        _topRightHUD.gameObject.SetActive(false);
        //PlayerCondition.Instance.GetComponent<PlayerTemperatureManager>().enabled = false;
        // PlayerCondition.Instance.GetComponent<PlayerDryingManager>().enabled = false;
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
            NarratorSpeechController.Instance.PostMessage("Press 'v' to interact. Press space to use a tool.");
        }
        LoadInitialScene();
    }

    void LoadInitialScene() {
        LevelChanger.ChangeLevel(_toScene, _sceneSpawnLocation);
    }

    void OnSceneUnloaded(Scene current) {
        // Turn on visual elements
        _player.enabled = true;
        _activeGridCell.gameObject.SetActive(true);
        _itemCursor.gameObject.SetActive(true);
        _inventoryContainer.gameObject.SetActive(true);
        _topRightHUD.gameObject.SetActive(true);
        // PlayerCondition.Instance.GetComponent<PlayerTemperatureManager>().enabled = true;
        // PlayerCondition.Instance.GetComponent<PlayerDryingManager>().enabled = true;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
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
