using UnityEngine;
public interface IWeighty : InteractInput.IInteractable, SaveData.ISaveable
{
    public WeightyObjectType WeightyObject { get; }
}

public class StoredWeightyObject 
{
    public WeightyObjectType Type;
    public SaveData SavedData;

    public StoredWeightyObject(IWeighty weighty)
    {
        this.Type = weighty.WeightyObject;
        this.SavedData = weighty.Save();
    }
}

[CreateAssetMenu(fileName = "WeightyObjectType", menuName = "Weighty/WeightyObjectType")]
public class WeightyObjectType : ScriptableObject
{
    public int Weight;
    public int StrengthRequired;
    public Sprite NSCarry;
    public Sprite EWCarry;
}
