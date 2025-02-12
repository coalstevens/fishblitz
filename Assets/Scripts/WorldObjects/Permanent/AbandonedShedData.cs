using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Abandoned Shed", menuName = "Buildings/Abandoned Shed")]
public class AbandonedShedData : ScriptableObject
{
    public int RepairProgress = 0;
    public bool AreVinesDestroyed = false;
    public List<string> NamesOfRepaired = new(); 
}