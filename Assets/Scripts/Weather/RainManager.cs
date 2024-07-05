using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum RainStates { Raining, NotRaining };

public class RainManager : Singleton<RainManager>
{
    public RainStates RainState = RainStates.Raining;
    public event Action<RainStates> RainStateChange; // Subscribe with events
    [SerializeField] private AudioClip _indoorRainSFX;
    [SerializeField] private AudioClip _outdoorRainSFX;
    private GameObject _rainParticleSystem;
    private Action _stopRainAudioHook;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Enable()
    {
        _rainParticleSystem.gameObject.SetActive(true);
        RainState = RainStates.Raining;
        RainStateChange.Invoke(RainState);
    }

    public void Disable()
    {
        _rainParticleSystem.gameObject.SetActive(false);
        RainState = RainStates.NotRaining;
        RainStateChange.Invoke(RainState);

    }
    private void StopRainAudio()
    {
        if (_stopRainAudioHook != null)
        {
            _stopRainAudioHook();
            _stopRainAudioHook = null;
        }
    }

    private void StartRainAudio()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Abandoned Shed":
                _stopRainAudioHook = AudioManager.Instance.PlayLoopingSFX(_indoorRainSFX, 0.5f, true);
                break;
            case "Outside":
                _stopRainAudioHook = AudioManager.Instance.PlayLoopingSFX(_outdoorRainSFX, 0.5f, true);
                break;
            default:
                StopRainAudio();
                break;
        }
    }

    // TODO If the rainsounds are the same between scenes, it should be a seamless transition
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Boot")
            return;
        if (RainState == RainStates.NotRaining)
            return;
        StopRainAudio();
        StartRainAudio();
    }
}
