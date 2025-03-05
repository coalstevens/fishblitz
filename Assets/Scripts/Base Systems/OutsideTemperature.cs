using UnityEngine;
using UnityEngine.SceneManagement;

public class OutsideTemperature : Singleton<OutsideTemperature>, IHeatSource
{
    private Grid _sceneGrid;
    private PlayerTemperatureManager _playerTemperatureManager;
    [SerializeField] private Logger _logger = new();
    public Temperature Temperature
    {
        get => WorldState.OutsideTemperature;
        set
        {
            if (value == WorldState.OutsideTemperature)
                return;
            WorldState.OutsideTemperature = value;
            _logger.Info($"Outside temperature changed to: {WorldState.OutsideTemperature}");
            OnTemperatureChange();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnTemperatureChange()
    {
        if (_sceneGrid != null)
            AddToAllHeatSensitives(_sceneGrid.transform);
    }

    private void AddToAllHeatSensitives(Transform worldGrid)
    {
        // Add to player
        if (_playerTemperatureManager != null)
            _playerTemperatureManager.AddHeatSource(this);

        // Add to world objects
        if (worldGrid != null)
            AddToAllChildHeatSensitives(worldGrid);
    }

    private void AddToAllChildHeatSensitives(Transform worldGrid)
    {
        // Find HeatSensitive world objects
        foreach (Transform _child in worldGrid)
        {
            if (_child.TryGetComponent<HeatSensitive>(out var _heatSensitive))
                _heatSensitive.AddHeatSource(this);
            AddToAllChildHeatSensitives(_child);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _playerTemperatureManager = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerTemperatureManager>();
        _sceneGrid = GameObject.FindFirstObjectByType<Grid>();
        if (_sceneGrid != null)
            AddToAllHeatSensitives(_sceneGrid.transform);
    }
}



