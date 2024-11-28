using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] AudioSource _musicPlayer; // Dedicated audiosource for playing music
    [SerializeField] private Transform _loopingSFXPlayerContainer; // Container for looping SFX audio sources, like rain
    [SerializeField] private Transform _SFXContainer; // Container for one-shot SFX audio sources
    [SerializeField] private Logger _logger = new();
    private Stack<AudioSource> _SFXPool = new();
    private Stack<AudioSource> _loopingSFXPool = new();
    private const float FADE_DURATION_SECS = 2f;

    private void Start()
    {
        foreach (var _source in _SFXContainer.GetComponentsInChildren<AudioSource>(true))
            _SFXPool.Push(_source);

        foreach (var _source in _loopingSFXPlayerContainer.GetComponentsInChildren<AudioSource>(true))
            _loopingSFXPool.Push(_source);

        _logger.Info($"SFX pool count: {_SFXPool.Count}    Looping pool count: {_loopingSFXPool.Count}");
    }

    public Action PlayMusic(AudioClip clip, float volume, bool fadeIn)
    {
        if (clip == null)
        {
            _logger.Warning("The music clip is null.");
            return null;
        }

        _musicPlayer.clip = clip;
        _musicPlayer.loop = true;

        if (fadeIn)
        {
            _musicPlayer.volume = 0;
            _musicPlayer.Play();
            StartCoroutine(FadeInAudio(_musicPlayer, FADE_DURATION_SECS, volume));
        }
        else
        {
            _musicPlayer.volume = volume;
            _musicPlayer.Play();
        }
        return () => DeactivateAudioSource(_musicPlayer, _loopingSFXPool);
    }

    /// <summary>
    /// Plays a looping SFX clip with optional fade in and fade out effects.
    /// </summary>
    public Action PlayLoopingSFX(AudioClip clip, float volume = 1, bool fadeIn = false, bool fadeOut = false, float fadeDurationSecs = FADE_DURATION_SECS)
    {
        if (clip == null)
        {
            _logger.Warning("The attached looping SFX clip is null.");
            return null;
        }

        AudioSource _source = GetPlayerFromPool(_loopingSFXPool);
        if (_source == null)
        {
            _logger.Warning("There are no more looping SFX audio sources available.");
            return null;
        }

        _logger.Info($"Playing looping SFX: {clip.name} with source: {_source.GetInstanceID()}");

        _source.clip = clip;
        _source.loop = true;

        if (fadeIn)
        {
            _source.volume = 0;
            _source.Play();
            StartCoroutine(FadeInAudio(_source, fadeDurationSecs, volume));
        }
        else
        {
            _source.volume = volume;
            _source.Play();
        }

        Action deactivateAction = () => 
        {
            _logger.Info($"Deactivating looping SFX source: {_source.GetInstanceID()}");
            if (fadeOut)
            {
                StartCoroutine(FadeOutAndDeactivateAudioSource(_source, fadeDurationSecs, _loopingSFXPool));
            }
            else
            {
                DeactivateAudioSource(_source, _loopingSFXPool);
            }
        };

        return deactivateAction;
    }

    private void DeactivateAudioSource(AudioSource source, Stack<AudioSource> pool)
    {
        if (pool.Contains(source)) {
            _logger.Warning("You must make the deactivate audio source callback null after use.");
            return;
        }
        source.Stop();
        source.gameObject.SetActive(false);
        pool.Push(source);
        _logger.Info($"Deactivated and returned source: {source.GetInstanceID()} to pool. Pool count: {pool.Count}");
    }

    public void PlaySFX(AudioClip clip, float volume = 1)
    {
        if (clip == null)
        {
            _logger.Warning("The SFX clip is null.");
            return;
        }
        AudioSource _source = GetPlayerFromPool(_SFXPool);
        if (_source == null)
        {
            _logger.Warning("There are no more SFX audio sources available.");
            return;
        }

        _logger.Info($"Playing SFX: {clip.name} with source: {_source.GetInstanceID()}");

        _source.clip = clip;
        _source.volume = volume;
        _source.loop = false;
        _source.Play();
        StartCoroutine(DisableAudioSourceAfterSound(_source, _SFXPool));
    }

    private AudioSource GetPlayerFromPool(Stack<AudioSource> pool)
    {
        if (pool.Count == 0)
            return null;
        AudioSource _source = pool.Pop();
        _source.gameObject.SetActive(true);
        _logger.Info($"Retrieved source: {_source.GetInstanceID()} from pool. Pool count: {pool.Count}");
        return _source;
    }

    private IEnumerator DisableAudioSourceAfterSound(AudioSource source, Stack<AudioSource> sourcePool)
    {
        yield return new WaitForSeconds(source.clip.length);
        sourcePool.Push(source);
        source.gameObject.SetActive(false);
        _logger.Info($"Disabled and returned source: {source.GetInstanceID()} to pool after playing. Pool count: {sourcePool.Count}");
    }

    private IEnumerator FadeInAudio(AudioSource source, float duration, float targetVolume)
    {
        float _startTime = Time.time;

        while (source.volume < targetVolume)
        {
            float elapsed = Time.time - _startTime;
            source.volume = Mathf.Lerp(0, targetVolume, elapsed / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }

    private IEnumerator FadeOutAndDeactivateAudioSource(AudioSource source, float duration, Stack<AudioSource> pool)
    {
        float _startTime = Time.time;
        float _startVolume = source.volume;

        while (source.volume > 0)
        {
            float elapsed = Time.time - _startTime;
            source.volume = math.lerp(_startVolume, 0, elapsed / duration);
            yield return null;
        }

        source.volume = 0;
        DeactivateAudioSource(source, pool);
    }

    // this function is kinda shit
    // there should be a more direct way for the caller to change the volume
    public bool TryAdjustVolume(AudioClip clip, float volume, float rampDuration) {
        // Combine all audio sources into one list
        var allSources = _loopingSFXPlayerContainer.GetComponentsInChildren<AudioSource>()
            .Concat(_SFXContainer.GetComponentsInChildren<AudioSource>());
        allSources.Append<AudioSource>(_musicPlayer);

        // Find the audio source with the matching clip
        AudioSource _sourcePlayingClip = allSources.FirstOrDefault(s => s.clip == clip);
        if (_sourcePlayingClip != null) {
            StartCoroutine(RampAudio(_sourcePlayingClip, volume, rampDuration));
            return true;
        }
        return false;
    }

    private IEnumerator RampAudio(AudioSource source, float volume, float rampDuration) {
        float _elapsed = 0;
        float _startVolume = source.volume;
        while (_elapsed < rampDuration) {
            source.volume = Mathf.Lerp(_startVolume, volume, _elapsed / rampDuration);
            _elapsed += Time.deltaTime;
            yield return null;
        }
        source.volume = volume;
    }
}