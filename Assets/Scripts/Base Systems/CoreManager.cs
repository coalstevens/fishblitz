using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreManager : Singleton<CoreManager>
{
    [Header("Config")]
    [SerializeField] private CoreManagerConfig _config;

    private static bool _hasLoadedFirstScene;
    private static string _pendingSpawnName;

    public static void SetPendingSpawn(string name)
    {
        _pendingSpawnName = name;
    }

    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 1)
            Debug.LogError($"[CoreManager] Found {players.Length} objects tagged 'Player' in scene '{gameObject.scene.name}'. Expected exactly 1. Found: {string.Join(", ", System.Array.ConvertAll(players, p => p.name))}");
#endif

        if (_config.ClearSaveOnStart)
        {
            ClearAllFilesInPersistentDataPath();
        }

        ResolveSpawnPosition();
        _hasLoadedFirstScene = true;

        InitializePlayerState();
        _config.RainManager.UpdateRainAudio();
    }

    private void Start()
    {
        WorldStateCalendar.Instance.UpdateWorldState();
    }

    private void ResolveSpawnPosition()
    {
        if (!_hasLoadedFirstScene && _config.UseDefaultSpawnOnFirstLoad)
        {
            string spawnName = _pendingSpawnName;
            if (string.IsNullOrEmpty(spawnName))
            {
                SceneNames currentScene = GetCurrentScene();
                spawnName = _config.SceneSpawnConfig.GetDefaultSpawnName(currentScene);
            }
            _config.PlayerData.SceneSpawnPosition = _config.SceneSpawnConfig.GetSpawnPosition(GetCurrentScene(), spawnName);
            _pendingSpawnName = null;
        }
    }

    private SceneNames GetCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (System.Enum.TryParse<SceneNames>(sceneName, out var scene))
            return scene;
        return SceneNames.Outside;
    }

    private void InitializePlayerState()
    {
        _config.PlayerData.WettingGameMinCounter = 0;
        _config.PlayerData.DryingPointsCounter = 0;
        _config.PlayerData.CounterToMatchAmbientGamemins = 0;
        _config.PlayerData.WetnessState.Value = _config.InitialWetnessState;
        _config.PlayerData.PlayerIsWet.Value = _config.InitialWetnessState == PlayerData.WetnessStates.Wet || _config.InitialWetnessState == PlayerData.WetnessStates.Drying;
        _config.PlayerData.ActualPlayerTemperature.Value = _config.InitialTemperature;
        _config.PlayerData.DryPlayerTemperature.Value = _config.PlayerData.PlayerIsWet.Value ? _config.InitialTemperature - 1 : _config.InitialTemperature;
        _config.PlayerData.CurrentEnergy.Value = _config.InitialEnergy;
        _config.PlayerData.IsPlayerSleeping = false;
        _config.PlayerInventory.ActiveItemSlot.Value = 0;
        _config.PlayerData.IsHoldingWheelBarrow.Value = false;
        _config.PlayerData.IsCarrying.Value = false;

        var playerCarry = FindFirstObjectByType<PlayerCarry>();
        if (playerCarry != null)
        {
            var stack = playerCarry.GetComponent<WeightyObjectStack>();
            stack.Clear();
        }
    }

    private void ClearAllFilesInPersistentDataPath()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath);
        foreach (string file in files)
            File.Delete(file);
    }
}
