using System.Collections.Generic;
using UnityEngine;
using ReactiveUnity;
using System;
using UnityEngine.SceneManagement;

public class PlayerDryingManager : MonoBehaviour, GameClock.ITickable
{
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private Rain _rainManager;
    
    // The drying points system is just math to enforce
    // the drying times amongst possible temperature changes
    private Dictionary<Temperature, int> _dryingTimesGameMins = new Dictionary<Temperature, int>
    {
        [Temperature.Hot] = 15,
        [Temperature.Warm] = 30,
        [Temperature.Neutral] = 2 * 60,
        [Temperature.Cold] = 6 * 60,
        [Temperature.Freezing] = 12 * 60 // 720
    };

    private const int DRYING_COMPLETE_POINTS = 720; // == freezing drying time 
    private const int DURATION_TO_GET_WET_GAMEMINS = 30;

    private string _sceneName;
    private WorldStateByCalendar.RainStates _rainState;
    private List<Action> _unsubscribeHooks = new List<Action>();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        _unsubscribeHooks.Add(WorldStateByCalendar.RainState.OnChange((_, curr) => OnRainStateChange(curr)));
        _unsubscribeHooks.Add(GameClock.Instance.GameMinute.OnChange((_, _) => OnGameMinuteTick()));
        _unsubscribeHooks.Add(_playerData.WetnessState.OnChange((_, _) => OnWetnessStateChange()));
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }
    public void OnGameMinuteTick()
    {
        HandleState();
    }

    private void OnWetnessStateChange()
    {
        _playerData.WettingGameMinCounter = 0;
        _playerData.DryingPointsCounter = 0;

        if (_playerData.WetnessState.Value == PlayerData.WetnessStates.Wet || _playerData.WetnessState.Value == PlayerData.WetnessStates.Drying)
        {
            _playerData.PlayerIsWet.Value = true;
            return;
        }
        _playerData.PlayerIsWet.Value = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _sceneName = scene.name;

        // Player can't be in the rain if not outside
        if (_sceneName != "Outside")
        {
            if (_playerData.WetnessState.Value == PlayerData.WetnessStates.Wet)
                EnterDrying();
            if (_playerData.WetnessState.Value == PlayerData.WetnessStates.Wetting)
                EnterDry();
        }
        else
        {
            HandleRain();
        }
    }

    private void OnRainStateChange(WorldStateByCalendar.RainStates newState)
    {
        _rainState = newState;
        HandleRain();
    }

    private void HandleRain()
    {
        if (_sceneName != "Outside")
            return;

        switch (_rainState)
        {
            case WorldStateByCalendar.RainStates.HeavyRain:
                // if wet stay wet
                // if wetting stay wetting
                if (_playerData.WetnessState.Value == PlayerData.WetnessStates.Dry)
                    EnterWetting();
                if (_playerData.WetnessState.Value == PlayerData.WetnessStates.Drying)
                    EnterWet();
                break;
            case WorldStateByCalendar.RainStates.NoRain:
                // if dry stay dry
                // if drying stay drying
                if (_playerData.WetnessState.Value == PlayerData.WetnessStates.Wet)
                    EnterDrying();
                if (_playerData.WetnessState.Value == PlayerData.WetnessStates.Wetting)
                    EnterDry();
                break;
        }
    }

    private void HandleState()
    {
        // can't dry/wet during sleep
        if (_playerData.IsPlayerSleeping)
            return;

        switch (_playerData.WetnessState.Value)
        {
            case PlayerData.WetnessStates.Wet:
                break;
            case PlayerData.WetnessStates.Dry:
                break;
            case PlayerData.WetnessStates.Drying:
                _playerData.DryingPointsCounter += GetDryingPoints(_playerData.ActualPlayerTemperature.Value);
                if (_playerData.DryingPointsCounter >= DRYING_COMPLETE_POINTS)
                {
                    EnterDry();
                }
                break;
            case PlayerData.WetnessStates.Wetting:
                _playerData.WettingGameMinCounter++;
                if (_playerData.WettingGameMinCounter >= DURATION_TO_GET_WET_GAMEMINS)
                {
                    EnterWet();
                }
                break;
        }
    }

    private void EnterDrying()
    {
        _playerData.WetnessState.Value = PlayerData.WetnessStates.Drying;
    }

    private void EnterWetting()
    {
        _playerData.WetnessState.Value = PlayerData.WetnessStates.Wetting;
    }

    private void EnterWet()
    {
        // ff you are drying, you are wet 
        if (!(_playerData.WetnessState.Value == PlayerData.WetnessStates.Drying))
            NarratorSpeechController.Instance.PostMessage("You are wet.");
        _playerData.WetnessState.Value = PlayerData.WetnessStates.Wet;
    }

    private void EnterDry()
    {
        // if you are wetting, you are dry
        if (!(_playerData.WetnessState.Value == PlayerData.WetnessStates.Wetting))
            NarratorSpeechController.Instance.PostMessage("You have dried off.");
        _playerData.WetnessState.Value = PlayerData.WetnessStates.Dry;
    }

    private int GetDryingPoints(Temperature currentTemperature)
    {
        if (_dryingTimesGameMins.TryGetValue(currentTemperature, out var _dryingTime))
        {
            return DRYING_COMPLETE_POINTS / _dryingTime;
        }
        else
        {
            Debug.LogError("The current temperature doesn't have an associated drying time.");
            return 0;
        }
    }
}
