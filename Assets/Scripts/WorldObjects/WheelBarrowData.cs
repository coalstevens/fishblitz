using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "WeightyObjectContainerData", menuName = "Weighty/WeightyObjectContainerData")]
public class WeightyObjectStackData : ScriptableObject
{ 
    public int WeightCapacity = 10;
    public int CurrentWeight = 0;
    public ReactiveStack<StoredWeightyObject> StoredObjects = new ReactiveStack<StoredWeightyObject>();
}