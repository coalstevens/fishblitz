using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using System.Collections;
using ReactiveUnity;

[RequireComponent(typeof(PlayerEnergyManager), typeof(PlayerTemperatureManager))]
public class PlayerDryingManager : MonoBehaviour, GameClock.ITickable, ISaveable
{
    public enum WetnessStates { Wet, Dry, Drying, Wetting };

    public string SaveableId => "PlayerWetness";

    public Reactive<WetnessStates> WetnessState = new Reactive<WetnessStates>(WetnessStates.Wet);
    public Reactive<bool> PlayerIsWet = new Reactive<bool>(true);
    public int DryingPointsCounter = 0;
    public int WettingGameMinCounter = 0;

    private PlayerEnergyManager _energyManager;
    private PlayerTemperatureManager _temperatureManager;

    private Dictionary<Temperature, int> _dryingTimesGameMins = new Dictionary<Temperature, int>
    {
        [Temperature.Hot] = 15,
        [Temperature.Warm] = 30,
        [Temperature.Normal] = 2 * 60,
        [Temperature.Cold] = 6 * 60,
        [Temperature.Freezing] = 12 * 60 // 720
    };

    private Dictionary<WetnessStates, string> _wetnessMessages = new Dictionary<WetnessStates, string>
    {
        [WetnessStates.Wet] = "your clothes are soaked.",
        [WetnessStates.Dry] = "you have dried off.",
        [WetnessStates.Drying] = "your damp clothes are drying.",
        [WetnessStates.Wetting] = "you are getting wet.",
    };

    private const int DRYING_COMPLETE_POINTS = 720; // == freezing drying time 
    private const int DURATION_TO_GET_WET_GAMEMINS = 30;

    private List<Action> _unsubscribeHooks = new List<Action>();

    [System.Serializable]
    private class State
    {
        public int WetnessState;
        public bool PlayerIsWet;
        public int DryingPointsCounter;
        public int WettingGameMinCounter;
    }

    private void OnEnable()
    {
        _energyManager = GetComponent<PlayerEnergyManager>();
        _temperatureManager = GetComponent<PlayerTemperatureManager>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameClock.Instance.OnGameMinuteTick += OnGameMinuteTick;
        _unsubscribeHooks.Add(WorldState.RainState.OnChange(_ => SetWetnessState()));
        _unsubscribeHooks.Add(WetnessState.OnChange((prev, curr) => OnWetnessStateChange(prev, curr)));
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

    private void OnWetnessStateChange(WetnessStates prev, WetnessStates curr)
    {
        PostStateChangeMessage(prev, curr);
        WettingGameMinCounter = 0;
        DryingPointsCounter = 0;

        PlayerIsWet.Value = 
            WetnessState.Value == WetnessStates.Wet || 
            WetnessState.Value == WetnessStates.Drying;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(WaitAndSetWetnessState());
    }

    private IEnumerator WaitAndSetWetnessState()
    {
        yield return null;
        SetWetnessState();
    }

    private void SetWetnessState()
    {
        string _sceneName = SceneManager.GetActiveScene().name;

        if (_sceneName != "Outside")
        {
            if (WetnessState.Value == WetnessStates.Wet)
                WetnessState.Value = WetnessStates.Drying;
            if (WetnessState.Value == WetnessStates.Wetting)
                WetnessState.Value = WetnessStates.Dry;
            return;
        }

        switch (WorldState.RainState.Value)
        {
            case WorldState.RainStates.HeavyRain:
                switch (WetnessState.Value)
                {
                    case WetnessStates.Dry:
                        WetnessState.Value = WetnessStates.Wetting;
                        break;
                    case WetnessStates.Drying:
                        WetnessState.Value = WetnessStates.Wet;
                        break;
                    case WetnessStates.Wet:
                    case WetnessStates.Wetting:
                    default: break;
                }
                break;
            case WorldState.RainStates.NoRain:
                switch (WetnessState.Value)
                {
                    case WetnessStates.Wet:
                        WetnessState.Value = WetnessStates.Drying;
                        break;
                    case WetnessStates.Wetting:
                        WetnessState.Value = WetnessStates.Dry;
                        break;
                    case WetnessStates.Dry:
                    case WetnessStates.Drying:
                    default: break;
                }
                break;
        }
    }

    private void HandleWetnessState()
    {
        if (_energyManager.IsPlayerSleeping)
            return;

        switch (WetnessState.Value)
        {
            case WetnessStates.Drying:
                DryingPointsCounter += GetDryingPoints(_temperatureManager.AmbientTemperature);
                if (DryingPointsCounter >= DRYING_COMPLETE_POINTS)
                    WetnessState.Value = WetnessStates.Dry;
                break;
            case WetnessStates.Wetting:
                WettingGameMinCounter++;
                if (WettingGameMinCounter >= DURATION_TO_GET_WET_GAMEMINS)
                    WetnessState.Value = WetnessStates.Wet;
                break;
            case WetnessStates.Wet:
            case WetnessStates.Dry:
                break;
            default:
                Debug.LogError("Invalid wetness state.");
                break;
        }
    }

    private void PostStateChangeMessage(WetnessStates prev, WetnessStates curr)
    {
        string message = curr switch
        {
            WetnessStates.Drying when prev == WetnessStates.Wet => _wetnessMessages[WetnessStates.Drying],
            WetnessStates.Dry when prev == WetnessStates.Wet || prev == WetnessStates.Drying => _wetnessMessages[WetnessStates.Dry],
            WetnessStates.Wet when prev == WetnessStates.Wetting || prev == WetnessStates.Dry => _wetnessMessages[WetnessStates.Wet],
            WetnessStates.Wetting when prev == WetnessStates.Dry => _wetnessMessages[WetnessStates.Wetting],
            _ => ""
        };

        if (!string.IsNullOrEmpty(message) && Narrator.Instance != null)
            Narrator.Instance.PostMessage(message);
    }

    private int GetDryingPoints(Temperature ambientTemperature)
    {
        if (_dryingTimesGameMins.TryGetValue(ambientTemperature, out var _dryingTime))
            return DRYING_COMPLETE_POINTS / _dryingTime;
        else
            Debug.LogError("The current temperature doesn't have an associated drying time.");
        return 0;
    }

    public string CaptureState()
    {
        var _state = new State
        {
            WetnessState = (int)WetnessState.Value,
            PlayerIsWet = PlayerIsWet.Value,
            DryingPointsCounter = DryingPointsCounter,
            WettingGameMinCounter = WettingGameMinCounter
        };
        return JsonConvert.SerializeObject(_state);
    }

    public void RestoreState(string json)
    {
        var _state = JsonConvert.DeserializeObject<State>(json);
        WetnessState.Value = (WetnessStates)_state.WetnessState;
        PlayerIsWet.Value = _state.PlayerIsWet;
        DryingPointsCounter = _state.DryingPointsCounter;
        WettingGameMinCounter = _state.WettingGameMinCounter;
    }

    public void ResetState()
    {
        WetnessState.Value = WetnessStates.Dry;
        PlayerIsWet.Value = false;
        DryingPointsCounter = 0;
        WettingGameMinCounter = 0;
    }
}
