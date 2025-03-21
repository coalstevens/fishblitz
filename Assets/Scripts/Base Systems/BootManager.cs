using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{
    private enum SpawnPositions
    {
        GameSpawn,
        AbandonedShed,
        Waterfall
    }
    [SerializeField] private bool _skipIntro = true;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] private RainAudio _rainManager;
    [SerializeField] private WorldStateCalendar _worldStateCalendar;

    [Header("Initial Scene Transition")]
    [SerializeField] private string _toScene;
    [SerializeField] private SpawnPositions _spawnPosition;
    [SerializeField] private bool _useCustomSpawn;
    [SerializeField] private Vector3 _customSpawnLocation;

    [Header("Initial Player State")]
    [SerializeField] private PlayerData.WetnessStates _initialWetnessState;
    [SerializeField] private Temperature _initialTemperature;
    [SerializeField] private int _initialEnergy;
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
        _playerData.WetnessState.Value = _initialWetnessState;
        _playerData.PlayerIsWet.Value = _initialWetnessState == PlayerData.WetnessStates.Wet || _initialWetnessState == PlayerData.WetnessStates.Drying;
        _playerData.ActualPlayerTemperature.Value = _initialTemperature;
        _playerData.DryPlayerTemperature.Value = _playerData.PlayerIsWet.Value ? _initialTemperature - 1 : _initialTemperature;
        _playerData.CurrentEnergy.Value = _initialEnergy;
        _playerData.IsPlayerSleeping = false;
        _playerInventory.ActiveItemSlot.Value = 0;
        _playerData.IsHoldingWheelBarrow.Value = false;
    }

    private IEnumerator OpeningDialogue()
    {
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
        string[] files = Directory.GetFiles(Application.persistentDataPath);

        foreach (string file in files)
            File.Delete(file);
    }
}
