using System.Collections.Generic;
using UnityEngine;

public class HeatSensitive : MonoBehaviour {
    [SerializeField] protected List<HeatSource> _ambientHeatSources = new();
    protected Temperature _ambientTemperature = (Temperature) 0;
    public virtual Temperature Temperature {
        get => _ambientTemperature;
    }
    public void AddHeatSource(HeatSource heatSource) {
        if (!_ambientHeatSources.Contains(heatSource)) {
            _ambientHeatSources.Add(heatSource);
        }
        SetAmbientTemperature();
    }

    public void RemoveHeatSource(HeatSource heatSource) {
        if (_ambientHeatSources.Contains(heatSource))
            _ambientHeatSources.Remove(heatSource);
        SetAmbientTemperature();
    }

    protected virtual void SetAmbientTemperature() {
        Temperature _hottestTemp = (Temperature) 0;
        foreach (var _source in _ambientHeatSources)
            if (_source.Temperature > _hottestTemp)
                _hottestTemp = _source.Temperature;
        _ambientTemperature = _hottestTemp; 
    }
}