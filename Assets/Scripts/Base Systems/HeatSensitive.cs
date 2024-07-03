using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

/// <summary>
/// HeatSensitive classes keep track of heatsources that are in-range.
/// The _ambientTemperature field is updated to be the hottest Temperature
/// of all the heatsources.
/// </summary>

public class HeatSensitive : MonoBehaviour {
    [SerializeField] protected List<HeatSource> _ambientHeatSources = new();
    protected Reactive<Temperature> _ambientTemperature = new Reactive<Temperature>((Temperature) 0);
    public virtual Temperature Temperature {
        get => _ambientTemperature.Value;
    }

    // called by HeatSources
    public void AddHeatSource(HeatSource heatSource) {
        if (!_ambientHeatSources.Contains(heatSource)) {
            _ambientHeatSources.Add(heatSource);
        }
        SetAmbientTemperature();
    }

    // called by HeatSources
    public void RemoveHeatSource(HeatSource heatSource) {
        if (_ambientHeatSources.Contains(heatSource))
            _ambientHeatSources.Remove(heatSource);
        SetAmbientTemperature();
    }

    protected virtual void SetAmbientTemperature() {
        _ambientTemperature.Value = GetTemperatureOfHottestSource();
    }
    
    protected Temperature GetTemperatureOfHottestSource() {
        Temperature _hottestTemperature = (Temperature) 0;
        List<HeatSource> _toBeRemoved = new();
        foreach (var _source in _ambientHeatSources) {
            if (_source == null) {
                _toBeRemoved.Add(_source);
                continue;
            }
            if (_source.Temperature > _hottestTemperature)
                _hottestTemperature = _source.Temperature;
        }

        foreach (var item in _toBeRemoved)
            _ambientHeatSources.Remove(item);

        return _hottestTemperature;
    }
}