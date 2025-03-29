using UnityEngine;
using UnityEngine.Assertions;

public interface IWeightyObjectContainer : InteractInput.IInteractable
{
    public WeightyObjectStack WeightyStack { get; }
}

public class WeightyObjectStack : MonoBehaviour
{
    [SerializeField] WeightyObjectStackData _data;

    private void Awake()
    {
        Assert.IsNotNull(_data);
    }

    public bool Push(StoredWeightyObject storedObject)
    {
        Assert.IsNotNull(storedObject);
        _data.CurrentWeight += storedObject.Type.Weight;
        _data.StoredObjects.Push(storedObject);
        return true;
    }

    public StoredWeightyObject Pop()
    {
        Assert.IsTrue(_data.StoredObjects.Count > 0);
        _data.CurrentWeight -= _data.StoredObjects.Peek().Type.Weight;
        return _data.StoredObjects.Pop();
    }

    public StoredWeightyObject Peek()
    {
        Assert.IsFalse(IsEmpty());
        return _data.StoredObjects.Peek();
    }

    public bool IsEmpty()
    {
        if (_data.StoredObjects.Count == 0)
        {
            Assert.IsTrue(_data.CurrentWeight == 0);
            return true;
        }
        return false;
    }

    public bool HasEnoughSpace(int weight)
    {
        Assert.IsTrue(weight > 0);
        return weight + _data.CurrentWeight <= _data.WeightCapacity;
    }
}
