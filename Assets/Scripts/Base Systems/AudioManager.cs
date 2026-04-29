using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private AudioSource _musicPlayer;
    [SerializeField] private AudioSource _oneShotSource;
    [SerializeField] private Logger _logger = new();
    private const float FADE_DURATION_SECS = 2f;

    public static Action PlayMusic(AudioClip clip, float volume, bool fadeIn)
    {
        if (clip == null)
        {
            Instance._logger.Warning("The music clip is null.");
            return null;
        }

        var source = Instance._musicPlayer;
        source.clip = clip;
        source.loop = true;

        if (fadeIn)
        {
            source.volume = 0;
            source.Play();
            Instance.StartCoroutine(FadeInAudio(source, FADE_DURATION_SECS, volume));
        }
        else
        {
            source.volume = volume;
            source.Play();
        }
        return () => source.Stop();
    }

    public static void PlaySFX(AudioSource source, SoundData sound)
    {
        if (source == null)
        {
            Instance._logger.Warning("The AudioSource is null.");
            return;
        }
        if (sound == null || sound.Clip == null)
        {
            Instance._logger.Warning("The SoundData or clip is null.");
            return;
        }

        Instance._logger.Info($"Playing SFX: {sound.Clip.name}");

        source.clip = sound.Clip;
        source.loop = false;

        float volume = sound.Volume;
        if (sound.UseVolumeVariation)
        {
            volume = Mathf.Clamp01(sound.Volume + UnityEngine.Random.Range(-sound.VolumeVariationAmount, sound.VolumeVariationAmount));
        }
        source.volume = volume;

        if (sound.UsePitchVariation)
        {
            source.pitch = 1f + UnityEngine.Random.Range(-sound.PitchVariationAmount, sound.PitchVariationAmount);
        }
        else
        {
            source.pitch = 1f;
        }

        source.Play();
    }

    public static Action PlayLoopingSFX(AudioSource source, SoundData sound)
    {
        if (source == null)
        {
            Instance._logger.Warning("The AudioSource is null.");
            return null;
        }
        if (sound == null || sound.Clip == null)
        {
            Instance._logger.Warning("The SoundData or clip is null.");
            return null;
        }

        Instance._logger.Info($"Playing looping SFX: {sound.Clip.name}");

        source.clip = sound.Clip;
        source.loop = sound.LoopSpacing <= 0;

        float volume = sound.Volume;
        if (sound.UseVolumeVariation)
        {
            volume = Mathf.Clamp01(sound.Volume + UnityEngine.Random.Range(-sound.VolumeVariationAmount, sound.VolumeVariationAmount));
        }
        source.volume = volume;

        if (sound.UsePitchVariation)
        {
            source.pitch = 1f + UnityEngine.Random.Range(-sound.PitchVariationAmount, sound.PitchVariationAmount);
        }
        else
        {
            source.pitch = 1f;
        }

        float baseVolume = volume;

        if (sound.FadeIn)
        {
            source.volume = 0;
            source.Play();
            Instance.StartCoroutine(FadeInAudio(source, sound.FadeDuration, baseVolume));
        }
        else
        {
            source.Play();
        }

        Coroutine organicLoopCoroutine = null;
        if (sound.LoopSpacing > 0)
        {
            organicLoopCoroutine = Instance.StartCoroutine(LoopWithOrganicVariation(source, sound, baseVolume));
        }

        Action stopAction = () =>
        {
            Instance._logger.Info($"Stopping looping SFX: {sound.Clip.name}");
            if (organicLoopCoroutine != null)
            {
                Instance.StopCoroutine(organicLoopCoroutine);
            }
            if (sound.FadeOut)
            {
                Instance.StartCoroutine(FadeOutAudio(source, sound.FadeDuration));
            }
            else
            {
                source.Stop();
            }
        };

        return stopAction;
    }

    private static IEnumerator LoopWithOrganicVariation(AudioSource source, SoundData sound, float baseVolume)
    {
        float clipLength = source.clip.length;
        float loopEndBuffer = 0.05f;
        float baseSpacing = sound.LoopSpacing;
        float variationPct = sound.LoopVariationPct;

        while (source != null && source.isPlaying)
        {
            if (source.time >= clipLength - loopEndBuffer)
            {
                float waitTime = baseSpacing * UnityEngine.Random.Range(1 - variationPct, 1 + variationPct);
                yield return new WaitForSeconds(waitTime);

                if (sound.UsePitchVariation)
                {
                    source.pitch = 1f + UnityEngine.Random.Range(-sound.PitchVariationAmount, sound.PitchVariationAmount);
                }
                if (sound.UseVolumeVariation)
                {
                    source.volume = Mathf.Clamp01(baseVolume + UnityEngine.Random.Range(-sound.VolumeVariationAmount, sound.VolumeVariationAmount));
                }

                source.PlayScheduled(AudioSettings.dspTime);
            }
            yield return null;
        }
    }

    private static IEnumerator FadeInAudio(AudioSource source, float duration, float targetVolume)
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

    private static IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        float startTime = Time.time;
        float startVolume = source.volume;

        while (source.volume > 0)
        {
            float elapsed = Time.time - startTime;
            source.volume = math.lerp(startVolume, 0, elapsed / duration);
            yield return null;
        }

        source.volume = 0;
        source.Stop();
    }
}