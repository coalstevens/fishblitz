using System;
using System.Collections.Generic;
using UnityEngine;

// Enum must go from cold->hot
// Some logic relies on the enum numeric value
public enum Temperature {Freezing, Cold, Neutral, Warm, Hot};

// This currently only works for non-moving heatsources and heatsensitives
// TODO: Dynamic object implementations
public class HeatEventSystem : MonoBehaviour {
    public static HeatEventSystem Instance;
    public event Action<int> HeatSourceRemoved;
    public event Action OutsideTemperatureChange;
    private Temperature _outsideTemperature = (Temperature) 0;
    public Temperature OutsideTemperature {
        get => _outsideTemperature;
        set {
            _outsideTemperature = value;
            OutsideTemperatureChange?.Invoke();
        }
    }
    
    private void Awake() {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void TriggerHeatSourceChangeEventWithinRadius(GameObject heatSource, Temperature newTemp, float affectedRadius) {
        Collider2D[] _collidersInRange = Physics2D.OverlapCircleAll(heatSource.transform.position, affectedRadius);
        foreach (var _collider in _collidersInRange) {
            if (_collider.transform.TryGetComponent<HeatSensitiveManager>(out var _heatSensitiveObject)) {
                _heatSensitiveObject.HandleLocalHeatSourceChange(heatSource.GetInstanceID(), newTemp);
            }
        }
    }

    public void RaiseHeatSourceRemovedEvent(GameObject source) {
        HeatSourceRemoved?.Invoke(source.GetInstanceID());
    }
}

