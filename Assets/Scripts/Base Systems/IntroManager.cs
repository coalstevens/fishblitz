using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    private enum SpawnPositions
    {
        GameSpawn,
        AbandonedShed,
        Waterfall
    }
    [SerializeField] private bool _skipIntro = true;
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] private RainAudio _rainManager;
    [SerializeField] private WorldStateCalendar _worldStateCalendar;

    [Header("Initial Scene Transition")]
    [SerializeField] private string _toScene;
    [SerializeField] private SpawnPositions _spawnPosition;
    [SerializeField] private bool _useCustomSpawn;
    [SerializeField] private Vector3 _customSpawnLocation;

    private Dictionary<SpawnPositions, Vector3> _spawnPositions = new Dictionary<SpawnPositions, Vector3> {
        { SpawnPositions.GameSpawn, new Vector3(-1.5f, -7f) },
        { SpawnPositions.AbandonedShed, new Vector3(37f, 37f) },
        { SpawnPositions.Waterfall, new Vector3(68f, -20f) }
    };

    private void Awake()
    {
        SetInitialPlayerState();
        ClearAllFilesInPersistentDataPath();
        StartCoroutine(OpeningDialogue());
    }

    private void Start()
    {
        _worldStateCalendar.UpdateWorldState();
        _rainManager.UpdateRainAudio();
    }

    private void SetInitialPlayerState()
    {
        var _energyManager = FindFirstObjectByType<PlayerEnergyManager>();
        var _dryingManager = FindFirstObjectByType<PlayerDryingManager>();
        var _temperatureManager = FindFirstObjectByType<PlayerTemperatureManager>();
        var _strengthManager = FindFirstObjectByType<PlayerStrength>();
        var _sceneData = FindFirstObjectByType<PlayerSceneData>();

        if (_energyManager != null) _energyManager.ResetToDefaults();
        if (_dryingManager != null) _dryingManager.ResetToDefaults();
        if (_temperatureManager != null) _temperatureManager.ResetToDefaults();
        if (_strengthManager != null) _strengthManager.ResetToDefaults();

        _playerInventory.ActiveItemSlot.Value = 0;

        var playerCarry = FindFirstObjectByType<PlayerCarry>();
        if (playerCarry != null)
        {
            playerCarry.IsCarrying.Value = false;
            playerCarry.CarriedStack.Clear();
        }

        var playerWheelBarrow = FindFirstObjectByType<PlayerWheelBarrow>();
        if (playerWheelBarrow != null)
        {
            playerWheelBarrow.IsHoldingWheelBarrow.Value = false;
            playerWheelBarrow.WheelBarrowStack.Clear();
        }
    }

    private IEnumerator OpeningDialogue()
    {
        yield return null;
        if (!_skipIntro)
        {
            yield return new WaitForSeconds(1f);
            Narrator.Instance.PostMessage("...");
            Narrator.Instance.PostMessage("everything feels heavy...");
            yield return new WaitUntil(() => Narrator.Instance.AreMessagesClear());
            yield return new WaitForSeconds(1f);
        }
        LoadInitialScene();
    }

    private void LoadInitialScene()
    {
        if (_useCustomSpawn)
            PlayerSceneData.PendingSpawnPosition = _customSpawnLocation;
        else
            PlayerSceneData.PendingSpawnPosition = _spawnPositions[_spawnPosition];
        PlayerSceneData.HasPendingSpawn = true;
        LevelChanger.ChangeLevel(_toScene);
    }

    private void ClearAllFilesInPersistentDataPath()
    {
        string[] files = System.IO.Directory.GetFiles(Application.persistentDataPath);
        foreach (string file in files)
            System.IO.File.Delete(file);
    }
}
