using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] AudioSource _musicPlayer; // Dedicated audisource for playing music
    [SerializeField] Transform _loopingSFXPlayerContainer; // Dedicated audiosource for looping SFX, like rain
    [SerializeField] Transform _SFXPlayerContainer; // Parent for a pool of SFX audio sources

    Stack<AudioSource> _SFXPool = new();
    Stack<AudioSource> _loopingSFXPool = new();
    private const float FADE_DURATION_SECS = 2f;

    private void Start()
    {
        foreach (var _source in _SFXPlayerContainer.GetComponentsInChildren<AudioSource>(true))
            _SFXPool.Push(_source);

        foreach (var _source in _loopingSFXPlayerContainer.GetComponentsInChildren<AudioSource>(true))
            _loopingSFXPool.Push(_source);
    }

    public void PlayMusic(AudioClip clip)
    {
        _musicPlayer.clip = clip;
        _musicPlayer.loop = true;
        _musicPlayer.Play();
    }

    /// <returns>Returns a hook to pause the </returns>
    public Action PlayLoopingSFX(AudioClip clip, float volume = 1, bool FadeIn = false)
    {
        AudioSource _source = GetPlayerFromPool(_loopingSFXPool);
        if (_source is null)
        {
            Debug.LogError("There are no more looping SFX audio sources available");
            return null;
        }
        _source.clip = clip;
        _source.loop = true;
        if (FadeIn)
        {
            _source.volume = 0;
            _source.Play();
            StartCoroutine(FadeInAudio(_source, FADE_DURATION_SECS, volume));
        }
        else
        {
            _source.volume = volume;
            _source.Play();
        }
        return () => DeactivateAudioSource(_source, _loopingSFXPool);
    }

    private void DeactivateAudioSource(AudioSource source, Stack<AudioSource> pool)
    {
        source.Stop();
        source.gameObject.SetActive(false);
        pool.Push(source);
    }

    public void PlaySFX(AudioClip clip, float volume = 1)
    {
        AudioSource _source = GetPlayerFromPool(_SFXPool);
        if (_source is null)
        {
            Debug.LogError("There are no more SFX audio sources available");
            return;
        }

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
        return _source;
    }

    private IEnumerator DisableAudioSourceAfterSound(AudioSource source, Stack<AudioSource> sourcePool)
    {
        yield return new WaitForSeconds(source.clip.length);
        source.gameObject.SetActive(false);
        sourcePool.Push(source);
    }
    public AudioSource audioSource;

    private IEnumerator FadeInAudio(AudioSource source, float duration, float targetVolume)
    {
        float startTime = Time.time;

        while (source.volume < targetVolume)
        {
            float elapsed = Time.time - startTime;
            source.volume = Mathf.Lerp(0, targetVolume, elapsed / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }
}
