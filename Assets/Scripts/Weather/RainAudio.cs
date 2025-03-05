using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using ReactiveUnity;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Rain", menuName = "Weather/Rain")]
public class RainAudio : ScriptableObject
{
    [SerializeField] private AudioClip _muffledRainSFX;
    [SerializeField] private float _muffledRainVolume = 1f;
    [SerializeField] private AudioClip _RainSFX;
    [SerializeField] private float _rainVolume = 0.3f;
    private Reactive<bool> _isRainMuffled = new Reactive<bool>(false);
    private Action _stopAudio;
    private List<Action> _unsubscribe = new();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        _unsubscribe.Add(WorldState.RainState.OnChange(curr => OnStateChange(curr)));
        _unsubscribe.Add(_isRainMuffled.OnChange(curr => OnMuffleChange(curr)));
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopRainAudio();
        foreach (var hook in _unsubscribe)
            hook();
    }

    public void OnStateChange(WorldState.RainStates curr)
    {
        switch (curr)
        {
            case WorldState.RainStates.HeavyRain:
                if (_stopAudio == null)
                    PlayRainAudio(_isRainMuffled.Value);
                break;
            case WorldState.RainStates.NoRain:
                StopRainAudio();
                break;
            default:
                Debug.LogError("Rain state does not exist.");
                break;
        }
    }

    private void OnMuffleChange(bool isMuffled)
    {
        if (WorldState.RainState.Value == WorldState.RainStates.NoRain) return;
        StopRainAudio();
        PlayRainAudio(isMuffled);
    }

    private void PlayRainAudio(bool isMuffled)
    {
        if (isMuffled)
            _stopAudio = AudioManager.Instance.PlayLoopingSFX(_muffledRainSFX, _muffledRainVolume);
        else
            _stopAudio = AudioManager.Instance.PlayLoopingSFX(_RainSFX, _rainVolume);
    }

    private void StopRainAudio()
    {
        if (_stopAudio != null)
        {
            _stopAudio();
            _stopAudio = null;
        }
    }

    private bool IsRainMuffled()
    {
        return SceneManager.GetActiveScene().name switch
        {
            "Abandoned Shed" => true,
            "SleepMenu" => true,
            "Outside" => false,
            "Boot" => false,
            _ => false
        };
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isRainMuffled.Value = IsRainMuffled();
        if (_stopAudio == null)
            PlayRainAudio(_isRainMuffled.Value);
    }
}
