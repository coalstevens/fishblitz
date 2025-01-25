using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "PlayerData")]
public class PlayerData : ScriptableObject
{
    public Vector3 SceneSpawnPosition = new Vector3(0, 0);
    [SerializeField] public CaptureLog BirdingLog;
    [SerializeField] public CaptureLog FishingLog;
}
