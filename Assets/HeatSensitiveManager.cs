using System.Collections.Generic;
using UnityEngine;

public class HeatSensitiveManager : MonoBehaviour {
    private Dictionary<int, Temperature> _localHeatSources = new();
    private Temperature _localTemperature = (Temperature) 0;
    public Temperature LocalTemperature {
        get => _localTemperature;
    }

    private void OnEnable() {
        HeatEventSystem.Instance.OutsideTemperatureChange += SetLocalTemperature;
        HeatEventSystem.Instance.HeatSourceRemoved += HandleHeatSourceRemoved;
    }

    private void OnDisable() {
        HeatEventSystem.Instance.OutsideTemperatureChange -= SetLocalTemperature;
        HeatEventSystem.Instance.HeatSourceRemoved -= HandleHeatSourceRemoved;
    }

    private void Start() {
        SetLocalTemperature();
    }

    public void HandleLocalHeatSourceChange(int eventSourceID, Temperature eventTemperature) {
        _localHeatSources[eventSourceID] = eventTemperature;
        SetLocalTemperature();
    }
    private void HandleHeatSourceRemoved(int eventSourceID) {
        _localHeatSources.Remove(eventSourceID);
    }
    
    private void SetLocalTemperature() {
        Temperature _hottestTemp = HeatEventSystem.Instance.OutsideTemperature;
        foreach (var kvp in _localHeatSources) {
            if ((int) kvp.Value > (int) _hottestTemp)
                _hottestTemp = kvp.Value;
        }
        _localTemperature = _hottestTemp; 
    }
}