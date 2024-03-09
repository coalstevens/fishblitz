using System;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

public class PlayerTemperatureManager : HeatSensitive, ITickable
{
    private Dictionary<Temperature, string> _temperatureChangeMessages = new Dictionary<Temperature, string> {
        [Temperature.Freezing] = "You are freezing.",
        [Temperature.Cold] = "You are cold.",
        [Temperature.Neutral] = "You are comfortable.",
        [Temperature.Warm] = "You are warm.",
        [Temperature.Hot] = "You are hot."
    }; 
    private const int DURATION_TO_MATCH_AMBIENT_GAMEMINS = 30;

    // State
    // private Temperature _ambientTemperature = Temperature.Cold;
    public Reactive<Temperature> _unadjustedPlayerTemperature = new Reactive<Temperature>(Temperature.Cold);
    public Reactive<Temperature> _adjustedPlayerTemperature = new Reactive<Temperature>(Temperature.Freezing);
    private List<Action> _unsubscribeHooks = new List<Action>();
    public int _counterToMatchAmbientGamemins = 0;
    private bool _playerIsWet = true;

    // References
    private PlayerDryingManager _playerDryingManager;
    public override Temperature Temperature {
        get {
            return _adjustedPlayerTemperature.Value;
        }
    }
    private void OnEnable() {
        _playerDryingManager = GetComponent<PlayerDryingManager>();
        _unsubscribeHooks.Add(GameClock.Instance.GameMinute.OnChange((_,_) => OnGameMinuteTick()));
        _unsubscribeHooks.Add(_playerDryingManager.PlayerIsWet.OnChange((_,curr) => OnWetnessChange(curr)));
        _unsubscribeHooks.Add(_unadjustedPlayerTemperature.OnChange((_,_) => OnUnadjustedTemperatureChange()));
        _unsubscribeHooks.Add(_adjustedPlayerTemperature.OnChange((_,_) => OnAdjustedTemperatureChange()));
    }

    private void OnDisable() {
        foreach (var hook in _unsubscribeHooks)
            hook();
    }

    private void OnWetnessChange(bool playerIsWet) {
        _playerIsWet = playerIsWet;
        SetAdjustedTemperature();
    }

    private void OnAmbientTemperatureChange(Temperature previousTemperature, Temperature currentTemperature) {
        if (previousTemperature < currentTemperature)
            _counterToMatchAmbientGamemins = 0;
    }

    private void OnAdjustedTemperatureChange() {
        if (!_temperatureChangeMessages.TryGetValue(_adjustedPlayerTemperature.Value, out var _message)) 
            Debug.LogError("There is no temp change message associated with the adjusted temp.");
        NarratorSpeechController.Instance.PostMessage(_message);
    }
    private void OnUnadjustedTemperatureChange() {
        _counterToMatchAmbientGamemins = 0;
        SetAdjustedTemperature();
    }
    

    public void OnGameMinuteTick() {
        // boot case
        if (_ambientHeatSources.Count == 0)
            return;
        
        // temp matches ambient already
        if (_unadjustedPlayerTemperature.Value == _ambientTemperature)
            return;

        // counter till switch to match ambient
        _counterToMatchAmbientGamemins++;
        if (_counterToMatchAmbientGamemins >= DURATION_TO_MATCH_AMBIENT_GAMEMINS) {
            _unadjustedPlayerTemperature.Value = _ambientTemperature;
        }
    }

    private void SetAdjustedTemperature() {
        // Player is dry
        if (!_playerIsWet) {  
            _adjustedPlayerTemperature.Value = _unadjustedPlayerTemperature.Value;
            return;
        }
        
        // Player is cold as can be already
        if (_unadjustedPlayerTemperature.Value == Temperature.Freezing) {
            _adjustedPlayerTemperature = _unadjustedPlayerTemperature;
            return;
        }

        // Player is wet, 1 step colder
        _adjustedPlayerTemperature.Value = _unadjustedPlayerTemperature.Value - 1;
    }
    protected override void SetAmbientTemperature() {
        Temperature _previousAmbientTemperature = _ambientTemperature;
        Temperature _hottestTemperature = (Temperature) 0;
        foreach (var _source in _ambientHeatSources)
            if (_source.Temperature > _hottestTemperature)
                _hottestTemperature = _source.Temperature;

        if (_hottestTemperature == _ambientTemperature)
            return;
        _ambientTemperature = _hottestTemperature;
        OnAmbientTemperatureChange(_previousAmbientTemperature, _ambientTemperature);
    }
}