using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalHeatSource : HeatSource {
    private Grid _sceneGrid;
    private PlayerTemperatureManager _playerTemperatureManager;
    public override Temperature Temperature {
        get => _temperature;
        set {
            if (value == _temperature)  
                return;
            _temperature = value;
            OnTemperatureChange();
        }
    }

    private void Start() {
        _playerTemperatureManager = PlayerCondition.Instance.GetComponent<PlayerTemperatureManager>();
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnTemperatureChange() {
        if(_sceneGrid != null)
            AddToAllHeatSensitives(_sceneGrid.transform);
    }

    private void AddToAllHeatSensitives(Transform worldGrid) {
        // Add to player
        _playerTemperatureManager.AddHeatSource(this);
        
        // Add to world objects
        AddToAllChildHeatSensitives(worldGrid);
    }

    private void AddToAllChildHeatSensitives(Transform worldGrid) {
        // Find HeatSensitive world objects
        foreach(Transform _child in worldGrid) {
            if (_child.TryGetComponent<HeatSensitive>(out var _heatSensitive))
                _heatSensitive.AddHeatSource(this);
            AddToAllChildHeatSensitives(_child);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        _sceneGrid = GameObject.FindObjectOfType<Grid>();
        if (_sceneGrid != null)
            AddToAllHeatSensitives(_sceneGrid.transform);
    }
}



