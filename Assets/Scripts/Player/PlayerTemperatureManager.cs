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
    [SerializeField] private PlayerData _playerData;
    private const int DURATION_TO_MATCH_AMBIENT_GAMEMINS = 30;
    private Dictionary<Temperature, string> _temperatureChangeMessages = new Dictionary<Temperature, string>
    {
        [Temperature.Freezing] = "you are freezing.",
        [Temperature.Cold] = "you are cold.",
        [Temperature.Neutral] = "you are comfortable.",
        [Temperature.Warm] = "you are warm.",
        [Temperature.Hot] = "you are hot."
    };

    private bool _skipNarratorMessage;
    private List<Action> _unsubscribeHooks = new List<Action>();

    public override Temperature Temperature
    {
        get => _playerData.ActualPlayerTemperature.Value;
    }

    private void OnEnable()
    {
        GameClock.Instance.OnGameMinuteTick += OnGameMinuteTick;
        _unsubscribeHooks.Add(_playerData.PlayerIsWet.OnChange(_ => UpdateActualTemperature()));
        _unsubscribeHooks.Add(_playerData.DryPlayerTemperature.OnChange(_ => UpdateActualTemperature()));
        _unsubscribeHooks.Add(_playerData.DryPlayerTemperature.OnChange(_ => ResetCounterToMatchAmbient()));
        _unsubscribeHooks.Add(_playerData.ActualPlayerTemperature.OnChange(_ => NarrateTemperatureChange()));
        _unsubscribeHooks.Add(_ambientTemperature.OnChange((prev, curr) => OnAmbientTemperatureChange(prev, curr)));
    }

    private void OnDisable()
    {
        GameClock.Instance.OnGameMinuteTick -= OnGameMinuteTick;
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

    private void UpdateActualTemperature()
    {
        // Player is dry
        if (!_playerData.PlayerIsWet.Value)
        {
            _playerData.ActualPlayerTemperature.Value = _playerData.DryPlayerTemperature.Value;
            return;
        }

        // Player is cold as can be already
        if (_playerData.DryPlayerTemperature.Value == Temperature.Freezing)
        {
            _playerData.ActualPlayerTemperature = _playerData.DryPlayerTemperature;
            return;
        }

        // Player is wet, 1 step colder
        _playerData.ActualPlayerTemperature.Value = _playerData.DryPlayerTemperature.Value - 1;
    }

    public void OnGameMinuteTick()
    {
        if
        (
            _ambientHeatSources.Count == 0 ||
            _playerData.IsPlayerSleeping || // no temp changes allowed during sleep
            _playerData.DryPlayerTemperature.Value == _ambientTemperature.Value
        )
            return;

        _playerData.CounterToMatchAmbientGamemins++;
        if (_playerData.CounterToMatchAmbientGamemins >= DURATION_TO_MATCH_AMBIENT_GAMEMINS)
            _playerData.DryPlayerTemperature.Value = _ambientTemperature.Value;
    }

    private void OnAmbientTemperatureChange(Temperature previousTemperature, Temperature currentTemperature)
    {
        // Player gets cold fast, and warm slow
        if (previousTemperature < currentTemperature)
            _playerData.CounterToMatchAmbientGamemins = 0;
    }

    private void NarrateTemperatureChange()
    {
        if (_skipNarratorMessage)
        {
            _skipNarratorMessage = false;
            Debug.Log("Player temperature narrator message skipped.");
            return;
        }

        // Post temperature change message
        if (!_temperatureChangeMessages.TryGetValue(_playerData.ActualPlayerTemperature.Value, out var _message))
            Debug.LogError("There is no temp change message associated with the adjusted temp.");
        NarratorSpeechController.Instance.PostMessage(_message);
    }

    private void ResetCounterToMatchAmbient()
    {
        _playerData.CounterToMatchAmbientGamemins = 0;
    }

    /// <summary>
    /// Attempts to set the player's dry temperature to match the ambient temperature instantly.
    /// </summary>
    /// <param name="_skipMessage"> If true, skips the narrator message on success.</param>
    /// <returns>
    /// Returns true if the player's dry temperature was updated to match the ambient temperature; 
    /// returns false if the temperatures were already equal.
    /// </returns>
    public bool TryUpdatePlayerTempInstantly(bool _skipMessage)
    {
        if (_playerData.DryPlayerTemperature.Value != _ambientTemperature.Value)
        {
            _skipMessage = true;
            _playerData.DryPlayerTemperature.Value = _ambientTemperature.Value;
            return true;
        }
        return false;
    }
}