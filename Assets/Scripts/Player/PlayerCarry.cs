using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using System.Collections;
using Newtonsoft.Json;
using ReactiveUnity;

[RequireComponent(typeof(PlayerStrength))]
[RequireComponent(typeof(PlayerInteraction))]
public class PlayerCarry : MonoBehaviour, ISaveable
{
    public string SaveableId => "PlayerCarry";

    public Reactive<bool> IsCarrying = new Reactive<bool>(false);

    [SerializeField] private WeightyObjectStack _carriedStack = new();
    [SerializeField] private WeightyObjectStackConfig _stackConfig;
    [SerializeField] private SoundData _putDownSound;
    private PlayerInteraction _playerInteraction;
    private PlayerMovement _playermovementController;
    private PlayerStrength _playerStrength;
    private GameObject _impermanent;
    private Grid _grid;
    private PlayerInput _playerInput;

    public WeightyObjectStack CarriedStack => _carriedStack;

    private void OnEnable()
    {
        _grid = FindFirstObjectByType<Grid>();
        Assert.IsNotNull(_grid);

        GameObject _player = GameObject.FindGameObjectWithTag("Player");
        Assert.IsNotNull(_player);

        _playerInput = _player.GetComponent<PlayerInput>();
        Assert.IsNotNull(_playerInput);

        _playermovementController = _player.GetComponent<PlayerMovement>();
        Assert.IsNotNull(_playermovementController);

        _impermanent = GameObject.FindGameObjectWithTag("Impermanent");
        Assert.IsNotNull(_impermanent);

        _playerInteraction = GetComponent<PlayerInteraction>();

        _playerStrength = GetComponent<PlayerStrength>();
    }

    public bool TryPickUpWeightyObject(IWeighty objectToPickup)
    {
        if (HasEnoughSpace(objectToPickup.WeightyObject.Weight) == false)
            return false;
        _playermovementController.PlayerState.Value = PlayerMovement.PlayerStates.PickingUp;

        StoredWeightyObject _objectToStore = new StoredWeightyObject(objectToPickup);

        IEnumerator DelayedPush(StoredWeightyObject objectToStore, float delay)
        {
            yield return new WaitForSeconds(delay);
            Push(objectToStore);
        }

        StartCoroutine(DelayedPush(_objectToStore, 0.06f * 4));

        return true;
    }

    public bool HasEnoughSpace(int weight)
    {
        return _carriedStack.HasEnoughSpace(weight);
    }

    public void Push(StoredWeightyObject objectToStore)
    {
        Assert.IsTrue(_carriedStack.HasEnoughSpace(objectToStore.Type.Weight));
        IsCarrying.Value = true;
        _carriedStack.Push(objectToStore);
        if (_stackConfig != null && _stackConfig.InsertSound != null)
            PlayerAudioManager.Instance.PlayOneShot(_stackConfig.InsertSound);
        _playerStrength.RegisterPickup(objectToStore.Record.PersistentID);
    }

    public void PutDown()
    {
        Assert.IsTrue(IsCarrying.Value);

        if (!_playerInteraction.TryGetUnoccupiedTileNearPlayer(out Vector3Int _spawnPosition))
            return;

        InstantiateWeightyObject(_carriedStack.Pop(), _spawnPosition);
        PlayerAudioManager.Instance.PlayOneShot(_putDownSound);
        IsCarrying.Value = !_carriedStack.IsEmpty();
    }

    public StoredWeightyObject Pop()
    {
        Assert.IsFalse(_carriedStack.IsEmpty());
        StoredWeightyObject _removedObject = _carriedStack.Pop();
        IsCarrying.Value = !_carriedStack.IsEmpty();
        return _removedObject;
    }

    public bool DropCarriedItemOnHit()
    {
        if (_carriedStack.IsEmpty())
            return false;

        if (TrySpawnNearPlayer(_carriedStack.Peek()) == false)
            return false;

        _carriedStack.Pop();
        IsCarrying.Value = !_carriedStack.IsEmpty();
        return true;
    }

    public bool DropStackItemOnHit(WeightyObjectStack stack)
    {
        if (stack.IsEmpty())
            return false;

        if (TrySpawnNearPlayer(stack.Peek()) == false)
            return false;

        stack.Pop();
        return true;
    }

    private bool TrySpawnNearPlayer(StoredWeightyObject storedObject)
    {
        if (!_playerInteraction.TryGetUnoccupiedTileNearPlayer(out Vector3Int _spawnPosition))
            return false;

        InstantiateWeightyObject(storedObject, _spawnPosition);
        PlayerAudioManager.Instance.PlayOneShot(_putDownSound);
        return true;
    }

    public StoredWeightyObject Peek()
    {
        return _carriedStack.Peek();
    }

    public int CarriedCount => _carriedStack.StoredCount;

    private void InstantiateWeightyObject(StoredWeightyObject carriedObject, Vector3Int spawnPosition)
    {
        GameObject prefab = Resources.Load<GameObject>("WorldObjects/" + carriedObject.Record.PrefabId);
        Vector3 _worldPos = SceneSpawner.CalculateWorldPosition(_grid, spawnPosition, prefab);
        carriedObject.Record.Position = _worldPos;

        IWeighty _spawnedObject = carriedObject.Record.Instantiate(_impermanent.transform).GetComponent<IWeighty>();
        carriedObject.Record.Restore(_spawnedObject);
    }

    public string CaptureState()
    {
        var _data = new StackContentsSaveData
        {
            Holding = IsCarrying.Value,
            Items = StoredWeightyObjectSaveData.CaptureAll(_carriedStack.StoredObjects)
        };
        return JsonConvert.SerializeObject(_data);
    }

    public void RestoreState(string json)
    {
        var _data = JsonConvert.DeserializeObject<StackContentsSaveData>(json);
        if (_data?.Items == null) return;

        _carriedStack.Clear();
        foreach (var _saveData in _data.Items)
        {
            var _storedObject = _saveData.Restore();
            if (_storedObject != null)
                _carriedStack.Push(_storedObject);
        }

        IsCarrying.Value = _data.Holding;
    }

    public void ResetState()
    {
        _carriedStack.Clear();
        IsCarrying.Value = false;
    }
}
