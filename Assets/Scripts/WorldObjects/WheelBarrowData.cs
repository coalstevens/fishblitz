using UnityEngine;

[CreateAssetMenu(fileName = "WeightyObjectContainerData", menuName = "Weighty/WeightyObjectContainerData")]
public class WeightyObjectStackData : ScriptableObject
{ 
    public int WeightCapacity = 10;
    public SoundData InsertSound;
    public SoundData RemoveSound;
}