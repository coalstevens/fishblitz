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
    public int TodaysProtein = 0;
    public int TodaysCarbs = 0;
    public int TodaysNutrients = 0;
    public const int PROTEIN_REQUIRED_DAILY = 100;
    public const int CARBS_REQUIRED_DAILY = 100;
    public const int NURTRIENTS_REQUIRD_DAILY = 100;

    [Header("Dryness")]
    public Reactive<bool> PlayerIsWet = new Reactive<bool>(true);
    public Reactive<Temperature> ActualPlayerTemperature = new Reactive<Temperature>(Temperature.Freezing);
    public Reactive<Temperature> DryPlayerTemperature = new Reactive<Temperature>(Temperature.Cold);
    public Reactive<WetnessStates> WetnessState = new Reactive<WetnessStates>(WetnessStates.Wet);
    public int DryingPointsCounter = 0;
    public int WettingGameMinCounter = 0;
    public int CounterToMatchAmbientGamemins = 0;
    public bool IsPlayerSleeping = false;
}

