using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Audio/SoundData")]
public class SoundData : ScriptableObject
{
    public AudioClip Clip;
    [Range(0, 1)] public float Volume = 1f;
    public bool IsLooping;

    public bool UsePitchVariation;
    [Range(0, 0.2f)] public float PitchVariationAmount = 0.02f;
    public bool UseVolumeVariation;
    [Range(0, 0.3f)] public float VolumeVariationAmount = 0.1f;

    public bool FadeIn;
    public bool FadeOut;
    public float FadeDuration = 2f;

    public float LoopSpacing = 0.05f;
    [Range(0, 1)] public float LoopVariationPct;
}