using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PlayerSleepQualityManager : MonoBehaviour 
{
    private int _minimumRecoveryFromSleep = 20;
    private int _minimumRecoveryFromNap = 5;
    private float _latestSleepScore = 0; 
    private const int GOOD_SLEEP_CURFEW_HOUR = 23;
    private const float PAST_CURFEW_REDUCER = 0.4f;
    private const float NAP_RECOVERY_FRACTION_OF_FULL_SLEEP = 0.3f;
    private Dictionary<Temperature, float> _temperatureBasedSleepReducers = new Dictionary<Temperature, float> {
        [Temperature.Freezing] = 0.1f,
        [Temperature.Cold] = 0.25f,
        [Temperature.Neutral] = 0f,
        [Temperature.Warm] = 0f,
        [Temperature.Hot] = 0.5f
    }; 
    private Dictionary<Temperature, int> _temperatureBasedAwakeHour = new Dictionary<Temperature, int> {
        [Temperature.Freezing] = 5,
        [Temperature.Cold] = 6,
        [Temperature.Neutral] = 7,
        [Temperature.Warm] = 7,
        [Temperature.Hot] = 6
    }; 
    private Dictionary<Temperature, string> _temperatureBasedAwakeMessage = new Dictionary<Temperature, string> {
        [Temperature.Freezing] = "The night was freezing, you slept poorly.",
        [Temperature.Cold] = "The night was cold, you managed to sleep ok.",
        [Temperature.Neutral] = "The night was comfortable, you slept well.",
        [Temperature.Warm] = "The night was warm, you slept well",
        [Temperature.Hot] = "The night was hot, you slept ok."
    }; 

    public float GetLastestSleepScore() {
        return _latestSleepScore;
    }
    
    public int GetAwakeHour() {
        if (!_temperatureBasedAwakeHour.TryGetValue(PlayerCondition.Instance.PlayerTemperature, out var _hour)) {
            Debug.LogError("There is no awake hour for the given temperature");
            return 7; // default to 7am
        }
        return _hour;
    }
    public string GetAwakeMessage() {
        if (!_temperatureBasedAwakeMessage.TryGetValue(PlayerCondition.Instance.PlayerTemperature, out var _message)) {
            Debug.LogError("There is no awake hour for the given temperature");
            return "";
        }
        return _message;
    }

    private void UpdateSleepScore() {
        float _newSleepScore = 1.00f;
        _newSleepScore -= GetPastCurfewReducer();
        _newSleepScore -= GetTemperatureReducer();
        _latestSleepScore = _newSleepScore;
    }

    private float GetNapScore() {
        return 1f - GetTemperatureReducer(); 
    }

    /// <returns> The players total energy after sleep </returns>
    public int GetEnergyFromSleep(int maxEnergy, float hungerRecoveryRatio, float sleepRecoveryRatio) {
        UpdateSleepScore();
        float _recovery = 0;
        _recovery += maxEnergy * hungerRecoveryRatio * PlayerCondition.Instance.PlayerFullnessPercentage;
        _recovery += maxEnergy * sleepRecoveryRatio * _latestSleepScore;

        return (_recovery < _minimumRecoveryFromSleep) ? _minimumRecoveryFromSleep : (int) _recovery;
    }
    
    /// <returns> The energy gained after the nap</returns>
    public int GetEnergyGainedFromNap(int maxEnergy, float hungerRecoveryRatio, float sleepRecoveryRatio) {
        float _potentialRecovery = maxEnergy * NAP_RECOVERY_FRACTION_OF_FULL_SLEEP;
        float _recovery = 0;
        _recovery += _potentialRecovery * hungerRecoveryRatio * PlayerCondition.Instance.PlayerFullnessPercentage;
        _recovery += _potentialRecovery * sleepRecoveryRatio * GetNapScore();

        return (_recovery < _minimumRecoveryFromNap) ? _minimumRecoveryFromNap : (int) _recovery;
    }

    private float GetPastCurfewReducer() {
        return GameClock.Instance.GameHour.Value >= GOOD_SLEEP_CURFEW_HOUR ? PAST_CURFEW_REDUCER : 0;
    }

    private float GetTemperatureReducer() {
        Temperature _playerTemp = PlayerCondition.Instance.PlayerTemperature;
        if (!_temperatureBasedSleepReducers.TryGetValue(_playerTemp, out var _reducer)) {
            Debug.LogError("There is no sleep reducer for this temperature");
            return 0f;
        }
        return _reducer;
    }
}