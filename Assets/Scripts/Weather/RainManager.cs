using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using ReactiveUnity;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Rain", menuName = "Weather/Rain")]
public class Rain : ScriptableObject
{
    [SerializeField] private AudioClip _muffledRainSFX;
    [SerializeField] private AudioClip _RainSFX;
    public enum States { HeavyRain, NoRain };
    public Reactive<States> State = new Reactive<States>(States.NoRain);
    private Reactive<bool> _isRainMuffled = new Reactive<bool>(false);
    private Action _stopAudio;
    private List<Action> _unsubscribe = new();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        _unsubscribe.Add(State.OnChange(curr => OnStateChange(curr)));
        _unsubscribe.Add(_isRainMuffled.OnChange(curr => OnMuffleChange(curr)));
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopRainAudio();
        foreach (var hook in _unsubscribe)
            hook();
    }

    private void OnStateChange(States curr)
    {
        switch (curr)
        {
            case States.HeavyRain:
                PlayRainAudio(_isRainMuffled.Value);
                break;
            case States.NoRain:
                StopRainAudio();
                break;
            default:
                Debug.LogError("Rain state does not exist.");
                break;
        }
    }

    private void OnMuffleChange(bool isMuffled)
    {
        if (State.Value == States.NoRain) return;
        StopRainAudio();
        PlayRainAudio(isMuffled);
    }

    private void PlayRainAudio(bool isMuffled)
    {
        Debug.Log("Rain audio play");
        if (isMuffled)
            _stopAudio = AudioManager.Instance.PlayLoopingSFX(_muffledRainSFX, 1, true);
        else
            _stopAudio = AudioManager.Instance.PlayLoopingSFX(_RainSFX, 0.5f, true);
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
            "Outside" => false,
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
