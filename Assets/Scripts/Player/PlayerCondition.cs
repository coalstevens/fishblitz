using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

// Player condition serves as a simple mediator to all its component scripts
public class PlayerCondition : Singleton<PlayerCondition>
{
    [SerializeField] private int _maxEnergy = 100;
    [SerializeField] private int _startingEnergy = 100;
    // Low energy reduces player movement 
    // Clothing keeps you warm
    // Clothing prevents you from getting wet
    public Temperature PlayerTemperature
    {
        get
        {
            return _playerTemperatureManager.Temperature;
        }
    }

    public Temperature AmbientTemperature
    {
        get
        {
            return _playerTemperatureManager.AmbientTemperature;
        }
    }

    public bool PlayerIsWet
    {
        get
        {
            return _playerDryingManager.PlayerIsWet.Value;
        }
    }

    public float PlayerFullnessPercentage
    {
        get
        {
            return _playerHungerManager.GetFullnessPercent();
        }
    }

    public float LatestSleepScore
    {
        get
        {
            return _playerSleepManager.GetLastestSleepScore();
        }
    }

    public Reactive<int> CurrentEnergy = new Reactive<int>(0);
    public int MaxEnergy => _maxEnergy;

    // Ratios, sum to 1
    private float _sleepRecoveryPercentageOfMax = 0.5f;
    private float _hungerRecoveryPercentageOfMax = 0.5f;

    private const int NAP_DURATION_GAMEHOURS = 3;
    private Logger _logger = new();

    private List<Action> _unsubscribeHooks = new List<Action>();
    private PlayerDryingManager _playerDryingManager;
    private PlayerHungerManager _playerHungerManager;
    private PlayerSleepQualityManager _playerSleepManager;
    private PlayerTemperatureManager _playerTemperatureManager;
    public bool PlayerIsAsleep = false;
    [SerializeField] private SleepMenu _sleepMenu;

    private void Start()
    {
        CurrentEnergy.Value = _startingEnergy;
    }

    void OnEnable()
    {
        _playerDryingManager = GetComponent<PlayerDryingManager>();
        _playerHungerManager = GetComponent<PlayerHungerManager>();
        _playerSleepManager = GetComponent<PlayerSleepQualityManager>();
        _playerTemperatureManager = GetComponent<PlayerTemperatureManager>();
        _unsubscribeHooks.Add(GameClock.Instance.GameHour.OnChange(curr => EndDay(curr)));
    }

    void OnDisable()
    {
        foreach (var hook in _unsubscribeHooks)
            hook();
    }

    private void EndDay(int currentHour)
    {
        if (currentHour == 0)
            _playerHungerManager.LogTodaysCalories();
    }

    public void Sleep()
    {
        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
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
        CurrentEnergy.Value = _playerSleepManager.GetEnergyFromSleep(_maxEnergy, _hungerRecoveryPercentageOfMax, _hungerRecoveryPercentageOfMax);
    }

    public void Nap()
    {
        GameClock.Instance.SkipTime(NAP_DURATION_GAMEHOURS * 60);
        CurrentEnergy.Value += _playerSleepManager.GetEnergyGainedFromNap(_maxEnergy, _hungerRecoveryPercentageOfMax, _sleepRecoveryPercentageOfMax);
        throw new NotImplementedException();
    }

    public void DepleteEnergy(int energy)
    {
        if (CurrentEnergy.Value >= energy)
        {
            CurrentEnergy.Value -= energy;
            _logger.Info("Energy depleted by " + energy + ". Current energy: " + CurrentEnergy.Value);
        }
        else if (CurrentEnergy.Value < energy && CurrentEnergy.Value > 0)
        {
            CurrentEnergy.Value = 0;
            _logger.Info("Energy insuffucient, this is the last player action");
            _logger.Info("Energy depleted by " + energy + ". Current energy: " + CurrentEnergy.Value);
        }
        else
        {
            _logger.Info("No energy left, player cannot perform this action");
        }
    }

    public void RecoverEnergy(int energy)
    {
        if (CurrentEnergy.Value + energy <= _maxEnergy)
        {
            CurrentEnergy.Value += energy;
            _logger.Info("Energy recovered by " + energy + ". Current energy: " + CurrentEnergy.Value);
        }
        else
        {
            CurrentEnergy.Value = _maxEnergy;
            _logger.Info("Energy recovered by " + energy + ". Current energy: " + CurrentEnergy.Value);
            _logger.Info("More than energy energy recovered, energy is now at max");
        }
    }

    public bool IsEnergyAvailable()
    {
        return CurrentEnergy.Value > 0;
    }
}
