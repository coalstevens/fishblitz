using System.Collections.Generic;
using UnityEngine;
using ReactiveUnity;
using System;
using UnityEngine.SceneManagement;

public class PlayerDryingManager : MonoBehaviour, ITickable
{
    // Drying
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

    public int _dryingPointsCounter;
    private const int DRYING_COMPLETE_POINTS = 720; // == freezing drying time 

    // Wetting
    private const int DURATION_TO_GET_WET_GAMEMINS = 30;
    private int _wettingGameMinCounter;

    public bool Paused = false;

    // State
    private enum WetnessStates { Wet, Dry, Drying, Wetting };
    [SerializeField] private Reactive<WetnessStates> _wetnessState = new Reactive<WetnessStates>(WetnessStates.Wet);
    public Reactive<bool> PlayerIsWet = new Reactive<bool>(true);
    private string _sceneName;
    private RainStates _rainState;

    // Reactive
    private List<Action> _unsubscribeHooks = new List<Action>();

    private void OnEnable()
    {
        RainManager.Instance.RainStateChange += OnRainStateChange;
        SceneManager.sceneLoaded += OnSceneLoaded;
        _unsubscribeHooks.Add(GameClock.Instance.GameMinute.OnChange((_, _) => OnGameMinuteTick()));
        _unsubscribeHooks.Add(_wetnessState.OnChange((_, _) => OnWetnessStateChange()));
    }

    private void OnDisable()
    {
        if (RainManager.Instance != null)
            RainManager.Instance.RainStateChange -= OnRainStateChange;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        foreach (var hook in _unsubscribeHooks)
            hook();
    }
    public void OnGameMinuteTick()
    {
        HandleState();
    }

    private void OnWetnessStateChange()
    {
        _wettingGameMinCounter = 0;
        _dryingPointsCounter = 0;

        if (_wetnessState.Value == WetnessStates.Wet || _wetnessState.Value == WetnessStates.Drying)
        {
            PlayerIsWet.Value = true;
            return;
        }
        PlayerIsWet.Value = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _sceneName = scene.name;
        // Keep defaults on boot
        if (_sceneName == "Boot")
            return;

        // Player can't be in the rain if not outside
        if (_sceneName != "Outside")
        {
            if (_wetnessState.Value == WetnessStates.Wet)
                EnterDrying();
            if (_wetnessState.Value == WetnessStates.Wetting)
                EnterDry();
        }
        else
        {
            HandleRain();
        }
    }

    private void OnRainStateChange(RainStates newState)
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
            case RainStates.Raining:
                // if wet stay wet
                // if wetting stay wetting
                if (_wetnessState.Value == WetnessStates.Dry)
                    EnterWetting();
                if (_wetnessState.Value == WetnessStates.Drying)
                    EnterWet();
                break;
            case RainStates.NotRaining:
                // if dry stay dry
                // if drying stay drying
                if (_wetnessState.Value == WetnessStates.Wet)
                    EnterDrying();
                if (_wetnessState.Value == WetnessStates.Wetting)
                    EnterDry();
                break;
        }
    }


    private void HandleState()
    {
        // can't dry/wet during sleep
        if (PlayerCondition.Instance.PlayerIsAsleep)
            return;

        switch (_wetnessState.Value)
        {
            case WetnessStates.Wet:
                break;
            case WetnessStates.Dry:
                break;
            case WetnessStates.Drying:
                _dryingPointsCounter += GetDryingPoints(PlayerCondition.Instance.PlayerTemperature);
                if (_dryingPointsCounter >= DRYING_COMPLETE_POINTS)
                {
                    EnterDry();
                }
                break;
            case WetnessStates.Wetting:
                _wettingGameMinCounter++;
                if (_wettingGameMinCounter >= DURATION_TO_GET_WET_GAMEMINS)
                {
                    EnterWet();
                }
                break;
        }
    }

    private void EnterDrying()
    {
        _wetnessState.Value = WetnessStates.Drying;
    }

    private void EnterWetting()
    {
        _wetnessState.Value = WetnessStates.Wetting;
    }

    private void EnterWet()
    {
        // ff you are drying, you are wet 
        if (!(_wetnessState.Value == WetnessStates.Drying))
            NarratorSpeechController.Instance.PostMessage("You are wet.");
        _wetnessState.Value = WetnessStates.Wet;
    }

    private void EnterDry()
    {
        // if you are wetting, you are dry
        if (!(_wetnessState.Value == WetnessStates.Wetting))
            NarratorSpeechController.Instance.PostMessage("You have dried off.");
        _wetnessState.Value = WetnessStates.Dry;
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
