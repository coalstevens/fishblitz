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
    private Action _stopRainAudioHook;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartRain()
    {
        if (RainState == RainStates.Raining)
            return;
        GameObject.FindGameObjectWithTag("Rain")?.SetActive(true);
        RainState = RainStates.Raining;
        RainStateChange.Invoke(RainState);
    }

    public void StopRain()
    {
        if (RainState == RainStates.NotRaining)
            return;
        GameObject.FindGameObjectWithTag("Rain")?.SetActive(false);
        StopRainAudio();
        RainState = RainStates.NotRaining;
        RainStateChange.Invoke(RainState);
        RainStateChange = null;
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
                _stopRainAudioHook = AudioManager.Instance.PlayLoopingSFX(_indoorRainSFX, 1, true);
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

        if (RainState == RainStates.Raining) {
            GameObject.FindGameObjectWithTag("Rain")?.SetActive(true);
            StopRainAudio();
            StartRainAudio();
            return;
        }

        if (RainState == RainStates.NotRaining) {
            GameObject.FindGameObjectWithTag("Rain")?.SetActive(false);
            return;
        }
    }
}
