using System.Collections.Generic;
using UnityEngine;
using ReactiveUnity;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
public class PlayerDryingManager : MonoBehaviour, GameClock.ITickable
{
    [SerializeField] private PlayerData _playerData;

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

    private Dictionary<PlayerData.WetnessStates, string> _wetnessMessages = new Dictionary<PlayerData.WetnessStates, string>
    {
        [PlayerData.WetnessStates.Wet] = "your clothes are soaked.",
        [PlayerData.WetnessStates.Dry] = "you have dried off.",
        [PlayerData.WetnessStates.Drying] = "your damp clothes are drying.",
        [PlayerData.WetnessStates.Wetting] = "you are getting wet.",
    };

    private const int DRYING_COMPLETE_POINTS = 720; // == freezing drying time 
    private const int DURATION_TO_GET_WET_GAMEMINS = 30;

    private List<Action> _unsubscribeHooks = new List<Action>();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameClock.Instance.OnGameMinuteTick += OnGameMinuteTick;
        _unsubscribeHooks.Add(WorldStateByCalendar.RainState.OnChange(_ => SetWetnessState()));
        _unsubscribeHooks.Add(_playerData.WetnessState.OnChange((prev, curr) => OnWetnessStateChange(prev, curr)));
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameClock.Instance.OnGameMinuteTick -= OnGameMinuteTick;
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

    public void OnGameMinuteTick()
    {
        HandleWetnessState();
    }

    private void OnWetnessStateChange(PlayerData.WetnessStates prev, PlayerData.WetnessStates curr)
    {
        PostStateChangeMessage(prev, curr);
        _playerData.WettingGameMinCounter = 0;
        _playerData.DryingPointsCounter = 0;

        _playerData.PlayerIsWet.Value = 
            _playerData.WetnessState.Value == PlayerData.WetnessStates.Wet || 
            _playerData.WetnessState.Value == PlayerData.WetnessStates.Drying;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(WaitAndSetWetnessState()); // wait for narrator to load in
    }

    private IEnumerator WaitAndSetWetnessState()
    {
        yield return null;
        SetWetnessState();
    }

    private void SetWetnessState()
    {
        string _sceneName = SceneManager.GetActiveScene().name;

        // Player can't be in the rain if not outside
        if (_sceneName != "Outside")
        {
            if (_playerData.WetnessState.Value == PlayerData.WetnessStates.Wet)
                _playerData.WetnessState.Value = PlayerData.WetnessStates.Drying;
            if (_playerData.WetnessState.Value == PlayerData.WetnessStates.Wetting)
                _playerData.WetnessState.Value = PlayerData.WetnessStates.Dry;
            return;
        }

        switch (WorldStateByCalendar.RainState.Value)
        {
            case WorldStateByCalendar.RainStates.HeavyRain:
                switch (_playerData.WetnessState.Value)
                {
                    case PlayerData.WetnessStates.Dry:
                        _playerData.WetnessState.Value = PlayerData.WetnessStates.Wetting;
                        break;
                    case PlayerData.WetnessStates.Drying:
                        _playerData.WetnessState.Value = PlayerData.WetnessStates.Wet;
                        break;
                    case PlayerData.WetnessStates.Wet: // if wet stay wet
                    case PlayerData.WetnessStates.Wetting: // if wetting stay wetting
                    default: break;
                }
                break;
            case WorldStateByCalendar.RainStates.NoRain:
                switch (_playerData.WetnessState.Value)
                {
                    case PlayerData.WetnessStates.Wet:
                        _playerData.WetnessState.Value = PlayerData.WetnessStates.Drying;
                        break;
                    case PlayerData.WetnessStates.Wetting:
                        _playerData.WetnessState.Value = PlayerData.WetnessStates.Dry;
                        break;
                    case PlayerData.WetnessStates.Dry: // if dry stay dry
                    case PlayerData.WetnessStates.Drying: // if drying stay drying 
                    default: break;
                }
                break;
        }
    }

    private void HandleWetnessState()
    {
        // can't dry/wet during sleep
        if (_playerData.IsPlayerSleeping)
            return;

        switch (_playerData.WetnessState.Value)
        {
            case PlayerData.WetnessStates.Drying:
                _playerData.DryingPointsCounter += GetDryingPoints(_playerData.ActualPlayerTemperature.Value);
                if (_playerData.DryingPointsCounter >= DRYING_COMPLETE_POINTS)
                    _playerData.WetnessState.Value = PlayerData.WetnessStates.Dry;
                break;
            case PlayerData.WetnessStates.Wetting:
                _playerData.WettingGameMinCounter++;
                if (_playerData.WettingGameMinCounter >= DURATION_TO_GET_WET_GAMEMINS)
                    _playerData.WetnessState.Value = PlayerData.WetnessStates.Wet;
                break;
            case PlayerData.WetnessStates.Wet:
            case PlayerData.WetnessStates.Dry:
                break;
            default:
                Debug.LogError("Invalid wetness state.");
                break;
        }
    }

    private void PostStateChangeMessage(PlayerData.WetnessStates prev, PlayerData.WetnessStates curr)
    {
        string message = curr switch
        {
            PlayerData.WetnessStates.Drying when prev == PlayerData.WetnessStates.Wet => _wetnessMessages[PlayerData.WetnessStates.Drying],
            PlayerData.WetnessStates.Dry when prev == PlayerData.WetnessStates.Wet || prev == PlayerData.WetnessStates.Drying => _wetnessMessages[PlayerData.WetnessStates.Dry],
            PlayerData.WetnessStates.Wet when prev == PlayerData.WetnessStates.Wetting || prev == PlayerData.WetnessStates.Dry => _wetnessMessages[PlayerData.WetnessStates.Wet],
            PlayerData.WetnessStates.Wetting when prev == PlayerData.WetnessStates.Dry => _wetnessMessages[PlayerData.WetnessStates.Wetting],
            _ => ""
        };

        if (!string.IsNullOrEmpty(message) && NarratorSpeechController.Instance != null)
            NarratorSpeechController.Instance.PostMessage(message);
    }

    private int GetDryingPoints(Temperature currentTemperature)
    {
        if (_dryingTimesGameMins.TryGetValue(currentTemperature, out var _dryingTime))
            return DRYING_COMPLETE_POINTS / _dryingTime;
        else
            Debug.LogError("The current temperature doesn't have an associated drying time.");
        return 0;
    }
}
