using System;
using System.Collections;
using System.Collections.Generic;
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
    public bool PlayerIsAsleep = false;
    [SerializeField] private SleepMenu _sleepMenu;

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
        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine() {
        // Transition to sleep menu
        PlayerIsAsleep = true;
        _sleepMenu.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(SleepMenu.FADE_DURATION_SEC);
        GameClock.Instance.PauseGame();

        // The player temp is instantly updated to the ambient temperature.
        // The player will not change temp or wetness while asleep.
        //    An example of why this is useful: If player lights a fire before going to bed 
        //    their sleep quality will improve, and they don't have to stand around waiting
        //    for the player temperature to match ambient.
        // The player does have to stand around to dry off. Getting into bed wet should be miserable.
        _playerTemperatureManager.TryUpdatePlayerTempInstantly(true);

        // Skip to awake hour
        GameClock.Instance.SkipToTime(GameClock.Instance.GameDay.Value + 1, _playerSleepManager.GetAwakeHour(PlayerTemperature), 0);

        // Delays and post awake message
        yield return new WaitForSecondsRealtime(2f);
        NarratorSpeechController.Instance.PostMessage(_playerSleepManager.GetAwakeMessage());
        yield return new WaitForSecondsRealtime(4f);

        // Transition back to game
        _sleepMenu.FadeOut();
        GameClock.Instance.ResumeGame();
        PlayerIsAsleep = false;
        yield return new WaitForSecondsRealtime(SleepMenu.FADE_DURATION_SEC);
        _sleepMenu.gameObject.SetActive(false);

        // Recover energy
        _currentEnergy = _playerSleepManager.GetEnergyFromSleep(_maxEnergy, _hungerRecoveryPercentageOfMax, _hungerRecoveryPercentageOfMax);
    }

    public void Nap() {
        GameClock.Instance.SkipTime(NAP_DURATION_GAMEHOURS * 60);
        _currentEnergy += _playerSleepManager.GetEnergyGainedFromNap(_maxEnergy, _hungerRecoveryPercentageOfMax, _sleepRecoveryPercentageOfMax);
        throw new NotImplementedException();
    }
}
