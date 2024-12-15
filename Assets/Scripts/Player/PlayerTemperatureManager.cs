using System;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

/// <summary>
/// Manages the player temperature.
/// dryTemperature is set to ambientTemperature after a duration.
/// if the player is dry, actualTemperature == dryTemperature
/// else actualTemperature == dryTemperature - 1 temp step
/// </summary>
public class PlayerTemperatureManager : HeatSensitive, GameClock.ITickable
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
    public Reactive<Temperature> _dryPlayerTemperature = new Reactive<Temperature>(Temperature.Cold);
    public Reactive<Temperature> _actualPlayerTemperature = new Reactive<Temperature>(Temperature.Freezing);
    private List<Action> _unsubscribeHooks = new List<Action>();
    public int _counterToMatchAmbientGamemins = 0;
    private bool _playerIsWet = true;
    private bool _skipMessage = false;

    // References
    private PlayerDryingManager _playerDryingManager;
    public override Temperature Temperature {
        get {
            return _actualPlayerTemperature.Value;
        }
    }

    public Temperature AmbientTemperature {
        get {
            return _ambientTemperature.Value;
        }
    }

    private void OnEnable() {
        _playerDryingManager = GetComponent<PlayerDryingManager>();
        _unsubscribeHooks.Add(GameClock.Instance.GameMinute.OnChange((_,_) => OnGameMinuteTick()));
        _unsubscribeHooks.Add(_playerDryingManager.PlayerIsWet.OnChange((_,curr) => OnWetnessChange(curr)));
        _unsubscribeHooks.Add(_dryPlayerTemperature.OnChange((_,_) => OnDryTemperatureChange()));
        _unsubscribeHooks.Add(_actualPlayerTemperature.OnChange((_,_) => OnActualTemperatureChange()));
        _unsubscribeHooks.Add(_ambientTemperature.OnChange((prev, curr) => OnAmbientTemperatureChange(prev, curr)));
    }

    private void OnDisable() {
        foreach (var hook in _unsubscribeHooks)
            hook();
    }

    private void UpdateActualTemperature() {
        // Player is dry
        if (!_playerIsWet) {  
            _actualPlayerTemperature.Value = _dryPlayerTemperature.Value;
            return;
        }
        
        // Player is cold as can be already
        if (_dryPlayerTemperature.Value == Temperature.Freezing) {
            _actualPlayerTemperature = _dryPlayerTemperature;
            return;
        }

        // Player is wet, 1 step colder
        _actualPlayerTemperature.Value = _dryPlayerTemperature.Value - 1;
    }

    public void OnGameMinuteTick() {
        // boot case
        if (_ambientHeatSources.Count == 0)
            return;

        // no temp changes during sleep
        if (PlayerCondition.Instance.PlayerIsAsleep)
            return;
        
        // temp matches ambient already
        if (_dryPlayerTemperature.Value == _ambientTemperature.Value)
            return;

        // counter till switch to match ambient
        _counterToMatchAmbientGamemins++;
        if (_counterToMatchAmbientGamemins >= DURATION_TO_MATCH_AMBIENT_GAMEMINS) {
            _dryPlayerTemperature.Value = _ambientTemperature.Value;
        }
    }

    private void OnWetnessChange(bool playerIsWet) {
        _playerIsWet = playerIsWet;
        UpdateActualTemperature();
    }

    private void OnAmbientTemperatureChange(Temperature previousTemperature, Temperature currentTemperature) {
        // Player gets cold fast, and warm slow
        if (previousTemperature < currentTemperature)
            _counterToMatchAmbientGamemins = 0;
    }

    private void OnActualTemperatureChange() {
        if (_skipMessage) {
            _skipMessage = false;
            Debug.Log("Player temperature narrator message skipped");
            return;
        }
        // Post temperature change message
        if (!_temperatureChangeMessages.TryGetValue(_actualPlayerTemperature.Value, out var _message)) 
            Debug.LogError("There is no temp change message associated with the adjusted temp.");
        NarratorSpeechController.Instance.PostMessage(_message);
    }

    private void OnDryTemperatureChange() {
        _counterToMatchAmbientGamemins = 0;
        UpdateActualTemperature();
    }
    
    /// <summary>
    /// Attempts to set the player's dry temperature to match the ambient temperature instantly.
    /// </summary>
    /// <param name="_skipMessage"> If true, skips the narrator message on success.</param>
    /// <returns>
    /// Returns true if the player's dry temperature was updated to match the ambient temperature; 
    /// returns false if the temperatures were already equal.
    /// </returns>
    public bool TryUpdatePlayerTempInstantly(bool _skipMessage) {
        if (_dryPlayerTemperature.Value != _ambientTemperature.Value) {
            _skipMessage = true;
            _dryPlayerTemperature.Value = _ambientTemperature.Value;
            return true;
        }
        return false;
    }
}