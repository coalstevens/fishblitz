
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
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private WeightyObjectStackData _playerCarriedObjects;
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
        GameStateManager.Initialize();
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
        _playerData.WettingGameMinCounter = 0;
        _playerData.DryingPointsCounter = 0;
        _playerData.CounterToMatchAmbientGamemins = 0;
        _playerData.WetnessState.Value = PlayerData.WetnessStates.Dry;
        _playerData.PlayerIsWet.Value = false;
        _playerData.ActualPlayerTemperature.Value = Temperature.Normal;
        _playerData.DryPlayerTemperature.Value = Temperature.Normal;
        _playerData.CurrentEnergy.Value = 100;
        _playerData.IsPlayerSleeping = false;
        _playerInventory.ActiveItemSlot.Value = 0;
        _playerData.IsHoldingWheelBarrow.Value = false;
        _playerData.IsCarrying.Value = false;
        _playerCarriedObjects.StoredObjects.Clear();
        _playerCarriedObjects.CurrentWeight = 0;
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
            _playerData.SceneSpawnPosition = _customSpawnLocation;
        else
            _playerData.SceneSpawnPosition = _spawnPositions[_spawnPosition];
        LevelChanger.ChangeLevel(_toScene);
    }

    private void ClearAllFilesInPersistentDataPath()
    {
        string[] files = System.IO.Directory.GetFiles(Application.persistentDataPath);
        foreach (string file in files)
            System.IO.File.Delete(file);
    }
}
