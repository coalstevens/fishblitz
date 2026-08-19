using UnityEngine;
public interface IWeighty : InteractInput.IInteractable, ISceneSaveable
{
    public WeightyObjectType WeightyObject { get; }
}

public class StoredWeightyObject 
{
    public WeightyObjectType Type;
    public SceneObjectRecord Record;

    public StoredWeightyObject(IWeighty weighty)
    {
        this.Type = weighty.WeightyObject;
        var weightyMb = weighty as MonoBehaviour;
        this.Record = SceneObjectRecord.Capture(weighty, weightyMb != null ? weightyMb.transform.position : Vector3.zero);
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
