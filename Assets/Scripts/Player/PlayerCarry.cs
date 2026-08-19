using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using System.Collections;
using ReactiveUnity;

[RequireComponent(typeof(PlayerStrength))]
public class PlayerCarry : MonoBehaviour
{
    public Reactive<bool> IsCarrying = new Reactive<bool>(false);

    [SerializeField] private WeightyObjectStack _carriedStack = new();
    [SerializeField] private WeightyObjectStackConfig _stackConfig;
    [SerializeField] private SoundData _putDownSound;
    private WorldObjectOccupancyMap _worldObjectOccupancyMap;
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

        _worldObjectOccupancyMap = _impermanent.GetComponent<WorldObjectOccupancyMap>();
        Assert.IsNotNull(_worldObjectOccupancyMap);

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

    public void PutDown(Vector3Int cursorLocationGrid)
    {
        Assert.IsTrue(IsCarrying.Value);

        if (!TryGetUnoccupiedPosition(cursorLocationGrid, out Vector3Int _spawnPosition))
            return;

        InstantiateWeightyObject(_carriedStack.Pop(), _spawnPosition);
        PlayerAudioManager.Instance.PlayOneShot(_putDownSound);
        IsCarrying.Value = !_carriedStack.IsEmpty();

        return;
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
        Vector3Int playerGrid = _grid.WorldToCell(transform.position);
        if (!TryGetUnoccupiedPosition(playerGrid, out Vector3Int _spawnPosition))
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

    private bool TryGetUnoccupiedPosition(Vector3Int cursorLocationGrid, out Vector3Int unoccupiedPosition)
    {
        Vector3Int[] _searchOrder = new Vector3Int[]
        {
            cursorLocationGrid,
            cursorLocationGrid + Vector3Int.up,
            cursorLocationGrid + Vector3Int.down,
            cursorLocationGrid + Vector3Int.left,
            cursorLocationGrid + Vector3Int.right,
            cursorLocationGrid + Vector3Int.up + Vector3Int.left,
            cursorLocationGrid + Vector3Int.up + Vector3Int.right,
            cursorLocationGrid + Vector3Int.down + Vector3Int.left,
            cursorLocationGrid + Vector3Int.down + Vector3Int.right
        };

        foreach (var _position in _searchOrder)
        {
            if (!_worldObjectOccupancyMap.CheckOccupied(_position))
            {
                unoccupiedPosition = _position;
                return true;
            }
        }

        unoccupiedPosition = default;
        return false;
    }
}
