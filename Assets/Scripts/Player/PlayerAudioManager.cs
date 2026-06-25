using System;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    private static PlayerAudioManager _instance;
    public static PlayerAudioManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<PlayerAudioManager>();
            return _instance;
        }
    }

    [SerializeField] private GameObject _audioSourceRoot;
    [SerializeField] private Logger _logger = new();
    private AudioSource[] _sources;
    private int _nextSourceIndex = 0;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        _sources = _audioSourceRoot.GetComponents<AudioSource>();
    }

    public void PlayOneShot(SoundData sound)
    {
        if (sound == null || sound.Clip == null)
        {
            _logger.Warning("SoundData or clip is null");
            return;
        }

        AudioSource source = GetAvailableSource();
        AudioManager.PlaySFX(source, sound);
    }

    public Action PlayLooping(SoundData sound)
    {
        if (sound == null || sound.Clip == null)
        {
            _logger.Warning("SoundData or clip is null");
            return null;
        }

        AudioSource source = GetAvailableSource();
        return AudioManager.PlayLoopingSFX(source, sound);
    }

    private AudioSource GetAvailableSource()
    {
        for (int i = 0; i < _sources.Length; i++)
        {
            int index = (_nextSourceIndex + i) % _sources.Length;
            if (!_sources[index].isPlaying)
            {
                _nextSourceIndex = (index + 1) % _sources.Length;
                return _sources[index];
            }
        }

        AudioSource newSource = _audioSourceRoot.AddComponent<AudioSource>();
        Array.Resize(ref _sources, _sources.Length + 1);
        _sources[^1] = newSource;
        return newSource;
    }
}
