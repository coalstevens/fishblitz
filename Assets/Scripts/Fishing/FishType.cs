using System;
using System.Collections.Generic;
using UnityEngine;

public class FishType : MonoBehaviour
{
    public List<string> CatchableSceneNames;
    public Inventory.Item CaughtItem;
    [SerializeField] public List<GameClock.Seasons> SpawnableSeasons = new();
    [SerializeField] public List<GameClock.DayPeriods> SpawnablePeriods = new();
    
    [Header("Minigame Round Settings")]
    public List<FishingRound> Rounds = new();
}

[Serializable]
public class FishingRound {
    public enum StackedTriggerType { none, doubles, mega };
    public float GameSpeed = 5f;
    public int NumberOfTriggers; // including final trigger at the very top
    public float MinimumTriggerSpacing; // triggers can't be closer together than this
    public float StackedTriggerSpacing; // custom gap for stacked trigger types
    public StackedTriggerType GameModifier;
    public bool HasOscillatingTriggers;
    public float OscillatingSpeed;
    [Range(0.0f, 1.0f)] public float OscillationLengthNormalized;
}
