using UnityEngine;
using UnityEngine.Assertions;
using ReactiveUnity;

public interface IWeightyObjectContainer : InteractInput.IInteractable
{
    public WeightyObjectStack WeightyStack { get; }
}

public class WeightyObjectStack : MonoBehaviour
{
    [SerializeField] public WeightyObjectStackData Data;

    public ReactiveStack<StoredWeightyObject> StoredObjects = new ReactiveStack<StoredWeightyObject>();
    public int CurrentWeight { get; private set; }

    private int _overriddenCapacity;
    private bool _hasCapacityOverride;

    public int EffectiveCapacity => _hasCapacityOverride ? _overriddenCapacity : (Data != null ? Data.WeightCapacity : 0);

    public void SetWeightCapacity(int capacity)
    {
        _overriddenCapacity = capacity;
        _hasCapacityOverride = true;
    }

    public void Clear()
    {
        StoredObjects.Clear();
        CurrentWeight = 0;
    }

    public bool Push(StoredWeightyObject storedObject)
    {
        Assert.IsNotNull(storedObject);
        CurrentWeight += storedObject.Type.Weight;
        StoredObjects.Push(storedObject);
        if (Data != null && Data.InsertSound != null)
            PlayerAudioManager.Instance.PlayOneShot(Data.InsertSound);
        return true;
    }

    public StoredWeightyObject Pop()
    {
        Assert.IsTrue(StoredObjects.Count > 0);
        CurrentWeight -= StoredObjects.Peek().Type.Weight;
        if (Data != null && Data.RemoveSound != null)
            PlayerAudioManager.Instance.PlayOneShot(Data.RemoveSound);
        return StoredObjects.Pop();
    }

    public StoredWeightyObject Peek()
    {
        Assert.IsFalse(IsEmpty());
        return StoredObjects.Peek();
    }

    public bool IsEmpty()
    {
        if (StoredObjects.Count == 0)
        {
            Assert.IsTrue(CurrentWeight == 0);
            return true;
        }
        return false;
    }

    public int StoredCount => StoredObjects.Count;

    public bool HasEnoughSpace(int weight)
    {
        Assert.IsTrue(weight > 0);
        return weight + CurrentWeight <= EffectiveCapacity;
    }
}
