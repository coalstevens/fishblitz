using UnityEngine;

[CreateAssetMenu(fileName = "CoreManagerConfig", menuName = "Core/Core Manager Config")]
public class CoreManagerConfig : ScriptableObject
{
    [Header("Shared References")]
    public PlayerData PlayerData;
    public Inventory PlayerInventory;
    public WeightyObjectStackData PlayerCarriedObjects;
    public RainAudio RainManager;
    public SceneSpawnConfig SceneSpawnConfig;

    [Header("Debug Options")]
    public bool ClearSaveOnStart;
    public bool UseDefaultSpawnOnFirstLoad = true;

    [Header("Initial Player State")]
    public PlayerData.WetnessStates InitialWetnessState;
    public Temperature InitialTemperature = Temperature.Normal;
    public int InitialEnergy = 100;
}
