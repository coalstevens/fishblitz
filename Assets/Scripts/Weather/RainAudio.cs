using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using ReactiveUnity;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RainAudio", menuName = "Weather/Rain")]
public class RainAudio : ScriptableObject
{
    private enum SoundType { Interior, HeavyExterior }

    [Serializable]
    private class SoundEffect
    {
        public SoundType _type;
        public AudioClip _sfxClip;
        public float _volume;
    }
    [SerializeField] List<SoundEffect> _rainSounds = new();
    private Action _stopAudio;
    private List<Action> _unsubscribe = new();
    private Reactive<SoundType> _currentSoundType = new Reactive<SoundType>(SoundType.HeavyExterior);

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        _unsubscribe.Add(WorldState.RainState.OnChange(_ => UpdateRainAudio()));
        _unsubscribe.Add(_currentSoundType.OnChange(_ => UpdateRainAudio()));
    }

    private void OnDisable()
    {
        StopRainAudio();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        foreach (var hook in _unsubscribe)
            hook();
        _unsubscribe.Clear();
    }

    public void UpdateRainAudio()
    {
        StopRainAudio();
        switch (WorldState.RainState.Value)
        {
            case WorldState.RainStates.HeavyRain:
                PlayRainAudio();
                break;
        }
    }

    private void PlayRainAudio()
    {
        SoundEffect _sfx = _rainSounds.Find(sfx => sfx._type == _currentSoundType.Value);
        if (_sfx == null)
        {
            Debug.LogError("No matching sound effect found for the current scene.");
            return;
        }

        _stopAudio = AudioManager.Instance.PlayLoopingSFX(_sfx._sfxClip, _sfx._volume);
    }

    private void StopRainAudio()
    {
        if (_stopAudio != null)
        {
            _stopAudio();
            _stopAudio = null;
        }
    }

    private SoundType GetSceneSoundType(string sceneName)
    {
        return sceneName switch
        {
            "Abandoned Shed" => SoundType.Interior,
            "SleepMenu" => SoundType.Interior,
            "Waterfall Cave" => SoundType.Interior,
            "Outside" => SoundType.HeavyExterior,
            "Boot" => SoundType.HeavyExterior,
            _ => SoundType.HeavyExterior
        };
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_stopAudio == null)
            UpdateRainAudio();
        if (mode != LoadSceneMode.Additive)
            _currentSoundType.Value = GetSceneSoundType(scene.name);
    }
}
