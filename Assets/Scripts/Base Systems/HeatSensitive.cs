using System;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

/// <summary>
/// HeatSensitive classes keep track of heat sources that are in range.
/// The _ambientTemperature field is updated to be the hottest Temperature
/// of all the heat sources.
/// </summary>
[Serializable]
public class HeatSensitive : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> _serializedMonoBehaviourHeatSources = new();
    [SerializeField] private List<ScriptableObject> _serializedScriptableObjectHeatSources = new();
    [SerializeField] protected Reactive<Temperature> _ambientTemperature = new Reactive<Temperature>((Temperature)0);
    protected List<IHeatSource> _ambientHeatSources = new(); // Actual working list of heat sources

    public virtual Temperature Temperature
    {
        get
        {
            SetAmbientTemperature();
            return _ambientTemperature.Value;
        }
    }

    private void Awake()
    {
        // Convert serialized lists into actual heat sources list
        _ambientHeatSources.Clear();
        foreach (var source in _serializedMonoBehaviourHeatSources)
            if (source is IHeatSource heatSource)
                _ambientHeatSources.Add(heatSource);
        foreach (var source in _serializedScriptableObjectHeatSources)
            if (source is IHeatSource heatSource)
                _ambientHeatSources.Add(heatSource);
        SetAmbientTemperature();
    }

    public void AddHeatSource(IHeatSource heatSource)
    {
        if (heatSource is MonoBehaviour heatSourceMB && !_serializedMonoBehaviourHeatSources.Contains(heatSourceMB))
            _serializedMonoBehaviourHeatSources.Add(heatSourceMB);
        if (heatSource is ScriptableObject heatSourceSO && !_serializedScriptableObjectHeatSources.Contains(heatSourceSO))
            _serializedScriptableObjectHeatSources.Add(heatSourceSO);
        if (!_ambientHeatSources.Contains(heatSource))
            _ambientHeatSources.Add(heatSource);
        SetAmbientTemperature();
    }

    public void RemoveHeatSource(IHeatSource heatSource)
    {
        if (heatSource is MonoBehaviour heatSourceMB && _serializedMonoBehaviourHeatSources.Contains(heatSourceMB))
            _serializedMonoBehaviourHeatSources.Remove(heatSourceMB);
        if (heatSource is ScriptableObject heatSourceSO && _serializedScriptableObjectHeatSources.Contains(heatSourceSO))
            _serializedScriptableObjectHeatSources.Remove(heatSourceSO);
        _ambientHeatSources.Remove(heatSource);
        SetAmbientTemperature();
    }

    protected virtual void SetAmbientTemperature()
    {
        _ambientTemperature.Value = GetTemperatureOfHottestSource();
    }

    protected Temperature GetTemperatureOfHottestSource()
    {
        Temperature _hottestTemperature = (Temperature)0;
        List<IHeatSource> _toBeRemoved = new();

        foreach (var _source in _ambientHeatSources)
        {
            if (_source == null)
            {
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