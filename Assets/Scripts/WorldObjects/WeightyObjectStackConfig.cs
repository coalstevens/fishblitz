using UnityEngine;

[CreateAssetMenu(fileName = "WeightyObjectContainerData", menuName = "Weighty/WeightyObjectContainerData")]
public class WeightyObjectStackConfig : ScriptableObject
{ 
    public int WeightCapacity = 10;
    public SoundData InsertSound;
    public SoundData RemoveSound;
}