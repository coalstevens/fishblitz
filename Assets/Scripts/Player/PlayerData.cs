using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "PlayerData")]
public class PlayerData : ScriptableObject
{
    public enum WetnessStates { Wet, Dry, Drying, Wetting };
    public Vector3 SceneSpawnPosition = new Vector3(0, 0);
    public string SceneOnAwake;

    [Header("Logs")]
    [SerializeField] public CaptureLog BirdingLog;
    [SerializeField] public CaptureLog FishingLog;

    [Header("Energy")]
    public Reactive<int> CurrentEnergy = new Reactive<int>(0);
    public int MaxEnergy = 100;

    [Header("Diet")]
    public float TodaysProtein = 0;
    public float TodaysCarbs = 0;
    public float TodaysNutrients = 0;
    public const float PROTEIN_REQUIRED_DAILY = 100;
    public const float CARBS_REQUIRED_DAILY = 100;
    public const float NUTRIENTS_REQUIRED_DAILY = 100;

    [Header("Dryness")] 
    public Reactive<bool> PlayerIsWet = new Reactive<bool>(true);
    public Reactive<Temperature> ActualPlayerTemperature = new Reactive<Temperature>(Temperature.Freezing);
    public Reactive<Temperature> DryPlayerTemperature = new Reactive<Temperature>(Temperature.Cold);
    public Reactive<WetnessStates> WetnessState = new Reactive<WetnessStates>(WetnessStates.Wet);
    public Reactive<bool> IsHoldingWheelBarrow = new Reactive<bool>(false);
    public Reactive<bool> IsCarrying = new Reactive<bool>(false);
    public int DryingPointsCounter = 0;
    public int WettingGameMinCounter = 0;
    public int CounterToMatchAmbientGamemins = 0;
    public bool IsPlayerSleeping = false;
    public int LastPlayerSleepTime = 0;
}

