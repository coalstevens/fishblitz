using System;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

// TODO get ambient temperature. Need to rework heateventsystem
class PlayerTemperatureManager : MonoBehaviour, ITickable
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
    private Temperature _ambientTemperature = Temperature.Cold;
    private Reactive<Temperature> _unadjustedPlayerTemperature = new Reactive<Temperature>(Temperature.Cold);
    private Reactive<Temperature> _adjustedPlayerTemperature = new Reactive<Temperature>(Temperature.Freezing);
    private List<Action> _unsubscribeHooks = new List<Action>();
    private int _counterToMatchAmbientGamemins = 0;
    private bool _playerIsWet;

    // References
    private PlayerDryingManager _playerDryingManager;

    public Temperature GetPlayerTemperature() {
        return _adjustedPlayerTemperature.Value;
    }
    private void Start() {
        _playerDryingManager = GetComponent<PlayerDryingManager>();
    } 
    private void OnEnable() {
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
        CheckForAdjustedTemperatureChange();
    }

    private void OnAmbientTemperatureChange(Temperature previousTemperature, Temperature currentTemperature) {
        if (previousTemperature < currentTemperature) {
            _counterToMatchAmbientGamemins = 0;
        }
    }

    private void OnUnadjustedTemperatureChange() {
        _counterToMatchAmbientGamemins = 0;
        CheckForAdjustedTemperatureChange();
    }
    
    private void OnAdjustedTemperatureChange() {
        if (!_temperatureChangeMessages.TryGetValue(_adjustedPlayerTemperature.Value, out var _message)) 
            Debug.LogError("There is no temp change message associated with the adjusted temp.");
        NarratorSpeechController.Instance.PostMessage(_message);
    }

    public void OnGameMinuteTick() {
        if (_unadjustedPlayerTemperature.Value == _ambientTemperature)
            return;

        _counterToMatchAmbientGamemins++;
        if (_counterToMatchAmbientGamemins >= DURATION_TO_MATCH_AMBIENT_GAMEMINS) {
            _unadjustedPlayerTemperature.Value = _ambientTemperature;
        }
    }

    private void CheckForAdjustedTemperatureChange() {
        // Being wet knocks player temp down one step
        if (!_playerIsWet) 
            _adjustedPlayerTemperature.Value = _unadjustedPlayerTemperature.Value;
        if (_unadjustedPlayerTemperature.Value == Temperature.Freezing) 
            _adjustedPlayerTemperature = _unadjustedPlayerTemperature;
        _adjustedPlayerTemperature.Value = _unadjustedPlayerTemperature.Value - 1;
    }
}