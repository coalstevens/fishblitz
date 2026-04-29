using UnityEngine;
using UnityEngine.Assertions;

public interface IWeightyObjectContainer : InteractInput.IInteractable
{
    public WeightyObjectStack WeightyStack { get; }
}

public class WeightyObjectStack : MonoBehaviour
{
    [SerializeField] public WeightyObjectStackData Data;
    [SerializeField] private AudioSource _audioSource;

    private void Awake()
    {
        Assert.IsNotNull(Data);
    }

    public bool Push(StoredWeightyObject storedObject)
    {
        Assert.IsNotNull(storedObject);
        Data.CurrentWeight += storedObject.Type.Weight;
        Data.StoredObjects.Push(storedObject);
        if (Data.InsertSound != null)
            AudioManager.PlaySFX(_audioSource, Data.InsertSound);
        return true;
    }

    public StoredWeightyObject Pop()
    {
        Assert.IsTrue(Data.StoredObjects.Count > 0);
        Data.CurrentWeight -= Data.StoredObjects.Peek().Type.Weight;
        if (Data.RemoveSound != null)
            AudioManager.PlaySFX(_audioSource, Data.RemoveSound);
        return Data.StoredObjects.Pop();
    }

    public StoredWeightyObject Peek()
    {
        Assert.IsFalse(IsEmpty());
        return Data.StoredObjects.Peek();
    }

    public bool IsEmpty()
    {
        if (Data.StoredObjects.Count == 0)
        {
            Assert.IsTrue(Data.CurrentWeight == 0);
            return true;
        }
        return false;
    }

    public int StoredCount => Data.StoredObjects.Count;

    public bool HasEnoughSpace(int weight)
    {
        Assert.IsTrue(weight > 0);
        return weight + Data.CurrentWeight <= Data.WeightCapacity;
    }
}
