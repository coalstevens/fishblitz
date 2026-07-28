using UnityEngine;
using UnityEngine.Assertions;
using ReactiveUnity;

public interface IWeightyObjectContainer : InteractInput.IInteractable
{
    public WeightyObjectStack WeightyStack { get; }
}

[System.Serializable]
public class WeightyObjectStack
{
    public ReactiveStack<StoredWeightyObject> StoredObjects = new ReactiveStack<StoredWeightyObject>();
    public int CurrentWeight { get; private set; }
    public int Capacity { get; set; }

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
        return true;
    }

    public StoredWeightyObject Pop()
    {
        Assert.IsTrue(StoredObjects.Count > 0);
        CurrentWeight -= StoredObjects.Peek().Type.Weight;
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
        return weight + CurrentWeight <= Capacity;
    }
}
