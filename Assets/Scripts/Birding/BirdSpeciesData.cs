using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBirdSpecies", menuName = "Birding/BirdSpecies")]
public class BirdSpeciesData : ScriptableObject
{
    [System.Serializable]
    public struct ClipOverride
    {
        public string OriginalName;
        public AnimationClip OverrideClip;
    }
    public string SpeciesName = "Chickadee";
    public GameObject Prefab;
    public List<BirdSpeciesData> FlockableSpecies = new();
    public Sprite Icon;
    public List<GameClock.Seasons> SpawnableSeasons = new();
    public List<GameClock.DayPeriods> SpawnablePeriods = new();
    public BirdBehaviourConfig BehaviourConfig;

    [Header("Animation")]
    public AnimatorController BaseAnimatorController;
    public List<ClipOverride> AnimationClips = new();

    [Header("Birding Game")]
    public AudioClip CaptureSound;
    public float SoundVolume;
    public PlayerData PlayerData;
}

