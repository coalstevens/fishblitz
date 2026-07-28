using UnityEngine;

[CreateAssetMenu(fileName = "BootstrapperConfig", menuName = "Core/Bootstrapper Config")]
public class BootstrapperConfig : ScriptableObject
{
    [Header("Persistent (DontDestroyOnLoad)")]
    public GameObject[] PersistentPrefabs;

    [Header("All Scenes")]
    public GameObject[] AllScenePrefabs;

    [Header("Inside Scenes")]
    public GameObject[] InsidePrefabs;

    [Header("Outside Scenes")]
    public GameObject[] OutsidePrefabs;
}
