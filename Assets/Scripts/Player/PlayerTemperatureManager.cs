using System;
using System.Collections.Generic;
using ReactiveUnity;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Manages the player temperature.
/// dryTemperature is set to ambientTemperature after a duration.
/// if the player is dry, actualTemperature == dryTemperature
/// else actualTemperature == dryTemperature - 1 temp step
/// </summary>
[RequireComponent(typeof(PlayerDryingManager), typeof(PlayerEnergyManager))]
public class PlayerTemperatureManager : HeatSensitive, GameClock.ITickable, ISaveableComponent
{
    public string ComponentId => "PlayerTemperature";

    public Reactive<Temperature> ActualPlayerTemperature = new Reactive<Temperature>(Temperature.Freezing);
    public Reactive<Temperature> DryPlayerTemperature = new Reactive<Temperature>(Temperature.Cold);
    public int CounterToMatchAmbientGamemins = 0;

    private PlayerDryingManager _dryingManager;
    private PlayerEnergyManager _energyManager;
    private const int DURATION_TO_SHIFT_TO_AMBIENT_GAMEMINS = 60;
    private Dictionary<Temperature, string> _temperatureChangeMessages = new Dictionary<Temperature, string>
    {
        [Temperature.Freezing] = "the cold is relentless.",
        [Temperature.Cold] = "chill seeps in.",
        [Temperature.Normal] = "you feel neither warmth nor cold.",
        [Temperature.Warm] = "you reach a comfortable warmth.",
        [Temperature.Hot] = "the heat presses down."
    };

    private bool _skipNarratorMessage;
    private List<Action> _unsubscribeHooks = new List<Action>();
    public Temperature AmbientTemperature => _ambientTemperature.Value;
    public override Temperature Temperature
    {
        get => ActualPlayerTemperature.Value;
    }

    [System.Serializable]
    private class State
    {
        public int ActualPlayerTemperature;
        public int DryPlayerTemperature;
        public int CounterToMatchAmbientGamemins;
    }

    private void OnEnable()
    {
        _dryingManager = GetComponent<PlayerDryingManager>();
        _energyManager = GetComponent<PlayerEnergyManager>();
        GameClock.Instance.OnGameMinuteTick += OnGameMinuteTick;
        _unsubscribeHooks.Add(_dryingManager.PlayerIsWet.OnChange(_ => UpdateActualTemperature()));
        _unsubscribeHooks.Add(DryPlayerTemperature.OnChange(_ => UpdateActualTemperature()));
        _unsubscribeHooks.Add(DryPlayerTemperature.OnChange(_ => ResetCounterToMatchAmbient()));
        _unsubscribeHooks.Add(ActualPlayerTemperature.OnChange(_ => NarrateTemperatureChange()));
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
        if (!_dryingManager.PlayerIsWet.Value)
        {
            ActualPlayerTemperature.Value = DryPlayerTemperature.Value;
            return;
        }

        if (DryPlayerTemperature.Value == Temperature.Freezing)
        {
            ActualPlayerTemperature = DryPlayerTemperature;
            return;
        }

        ActualPlayerTemperature.Value = DryPlayerTemperature.Value - 1;
    }

    public void OnGameMinuteTick()
    {
        if
        (
            _ambientHeatSources.Count == 0 ||
            _energyManager.IsPlayerSleeping ||
            DryPlayerTemperature.Value == _ambientTemperature.Value
        )
            return;

        CounterToMatchAmbientGamemins++;
        if (CounterToMatchAmbientGamemins >= DURATION_TO_SHIFT_TO_AMBIENT_GAMEMINS) {
            if(_ambientTemperature.Value > DryPlayerTemperature.Value)
                DryPlayerTemperature.Value++;
            else
                DryPlayerTemperature.Value--;
        } 
    }

    private void OnAmbientTemperatureChange(Temperature previousTemperature, Temperature currentTemperature)
    {
        if (previousTemperature < currentTemperature)
            CounterToMatchAmbientGamemins = 0;
    }

    private void NarrateTemperatureChange()
    {
        if (_skipNarratorMessage)
        {
            _skipNarratorMessage = false;
            Debug.Log("Player temperature narrator message skipped.");
            return;
        }

        if (!_temperatureChangeMessages.TryGetValue(ActualPlayerTemperature.Value, out var _message))
            Debug.LogError("There is no temp change message associated with the adjusted temp.");
        Narrator.Instance.PostMessage(_message);
    }

    private void ResetCounterToMatchAmbient()
    {
        CounterToMatchAmbientGamemins = 0;
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
        if (DryPlayerTemperature.Value != _ambientTemperature.Value)
        {
            _skipMessage = true;
            DryPlayerTemperature.Value = _ambientTemperature.Value;
            return true;
        }
        return false;
    }

    public string CaptureStateAsJson()
    {
        var _state = new State
        {
            ActualPlayerTemperature = (int)ActualPlayerTemperature.Value,
            DryPlayerTemperature = (int)DryPlayerTemperature.Value,
            CounterToMatchAmbientGamemins = CounterToMatchAmbientGamemins
        };
        return JsonConvert.SerializeObject(_state);
    }

    public void RestoreStateFromJson(string json)
    {
        var _state = JsonConvert.DeserializeObject<State>(json);
        ActualPlayerTemperature.Value = (Temperature)_state.ActualPlayerTemperature;
        DryPlayerTemperature.Value = (Temperature)_state.DryPlayerTemperature;
        CounterToMatchAmbientGamemins = _state.CounterToMatchAmbientGamemins;
    }

    public void ResetToDefaults()
    {
        ActualPlayerTemperature.Value = Temperature.Normal;
        DryPlayerTemperature.Value = Temperature.Normal;
        CounterToMatchAmbientGamemins = 0;
    }
}
