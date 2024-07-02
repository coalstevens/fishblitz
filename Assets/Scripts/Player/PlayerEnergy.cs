using System;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

// Player condition serves as a simple mediator to all its component scripts
public class PlayerCondition : Singleton<PlayerCondition>
{
    // Low energy reduces player movement 
    // Clothing keeps you warm
    // Clothing prevents you from getting wet
    public Temperature PlayerTemperature {
        get {
            return _playerTemperatureManager.Temperature;
        }
    }
    
    public Temperature AmbientTemperature {
        get {
            return _playerTemperatureManager.AmbientTemperature;
        }
    }
    
    public bool PlayerIsWet {
        get {
            return _playerDryingManager.PlayerIsWet.Value;
        }
    }
    
    public float PlayerFullnessPercentage {
        get {
            return _playerHungerManager.GetFullnessPercent();
        }
    }

    public float LatestSleepScore {
        get {
            return _playerSleepManager.GetLastestSleepScore();
        }
    }

    private int _maxEnergy = 100;
    private int _currentEnergy = 20;

    // Ratios, sum to 1
    private float _sleepRecoveryPercentageOfMax = 0.5f;
    private float _hungerRecoveryPercentageOfMax = 0.5f;

    private const int NAP_DURATION_GAMEHOURS = 3; 

    private List<Action> _unsubscribeHooks = new List<Action>();
    private PlayerDryingManager _playerDryingManager;   
    private PlayerHungerManager _playerHungerManager;
    private PlayerSleepQualityManager _playerSleepManager;
    private PlayerTemperatureManager _playerTemperatureManager;

    void OnEnable() {
        _playerDryingManager = GetComponent<PlayerDryingManager>();
        _playerHungerManager = GetComponent<PlayerHungerManager>();
        _playerSleepManager = GetComponent<PlayerSleepQualityManager>();
        _playerTemperatureManager = GetComponent<PlayerTemperatureManager>();
        _unsubscribeHooks.Add(GameClock.Instance.GameHour.When((_, currentHour) => currentHour == 0, (_,_) => EndDay()));
    }

    void OnDisable() {
        foreach (var hook in _unsubscribeHooks)
            hook();
    }

    private void EndDay() {
        _playerHungerManager.LogTodaysCalories();
    }
    public void Sleep() {
        // Pause time counters and skip time
        _playerTemperatureManager.Paused = true;
        _playerDryingManager.Paused = true;
        GameClock.Instance.SkipToTime(GameClock.Instance.GameDay.Value + 1, _playerSleepManager.GetAwakeHour(), 0);
        _playerTemperatureManager.Paused = false;
        _playerDryingManager.Paused = false;

        NarratorSpeechController.Instance.PostMessage(_playerSleepManager.GetAwakeMessage());
        _currentEnergy = _playerSleepManager.GetEnergyFromSleep(_maxEnergy, _hungerRecoveryPercentageOfMax, _hungerRecoveryPercentageOfMax);
    }
    public void Nap() {
        GameClock.Instance.SkipTime(NAP_DURATION_GAMEHOURS * 60);
        _currentEnergy += _playerSleepManager.GetEnergyGainedFromNap(_maxEnergy, _hungerRecoveryPercentageOfMax, _sleepRecoveryPercentageOfMax);
    }
}
