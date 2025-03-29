using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;

public class PlayerCarry : MonoBehaviour
{
    [SerializeField] private PlayerData _playerData;
    private WorldObjectOccupancyMap _worldObjectOccupancyMap;
    private WeightyObjectStack _carriedObjects;  
    private GameObject _impermanent;
    private Grid _grid;
    private PlayerInput _playerInput;

    private void OnEnable()
    {
        _grid = FindFirstObjectByType<Grid>();
        Assert.IsNotNull(_grid);

        GameObject _player = GameObject.FindGameObjectWithTag("Player");
        Assert.IsNotNull(_player);

        _playerInput = _player.GetComponent<PlayerInput>();
        Assert.IsNotNull(_playerInput);

        _impermanent = GameObject.FindGameObjectWithTag("Impermanent");
        Assert.IsNotNull(_impermanent);

        _worldObjectOccupancyMap = _impermanent.GetComponent<WorldObjectOccupancyMap>();
        Assert.IsNotNull(_worldObjectOccupancyMap);

        _carriedObjects = GetComponent<WeightyObjectStack>();
        Assert.IsNotNull(_carriedObjects);
        Assert.IsNotNull(_playerData);
    }

    public bool TryPickUpWeightyObject(IWeighty objectToPickup)
    {
        if (HasEnoughSpace(objectToPickup.WeightyObject.Weight) == false)
            return false;

        Push(new StoredWeightyObject(objectToPickup));
        // TODO run pickup animation

        return true;
    }

    public bool HasEnoughSpace(int weight)
    {
        return _carriedObjects.HasEnoughSpace(weight);
    }

    public void Push(StoredWeightyObject objectToStore)
    {
        Assert.IsTrue(_carriedObjects.HasEnoughSpace(objectToStore.Type.Weight));
        _playerData.IsCarrying.Value = true;
        _carriedObjects.Push(objectToStore);
    }

    public void PutDown(Vector3Int cursorLocationGrid)
    {
        Assert.IsTrue(_playerData.IsCarrying.Value);

        if (!TryGetUnoccupiedPosition(cursorLocationGrid, out Vector3Int _spawnPosition))
            return;

        InstantiateWeightyObject(_carriedObjects.Pop(), _spawnPosition);
        _playerData.IsCarrying.Value = !_carriedObjects.IsEmpty();

        return;
    }

    public StoredWeightyObject Pop()
    {
        Assert.IsFalse(_carriedObjects.IsEmpty());
        StoredWeightyObject _removedObject = _carriedObjects.Pop();
        _playerData.IsCarrying.Value = !_carriedObjects.IsEmpty();
        return _removedObject;
    }

    public StoredWeightyObject Peek()
    {
        return _carriedObjects.Peek();
    }

    private void InstantiateWeightyObject(StoredWeightyObject carriedObject, Vector3Int spawnPosition)
    {
        Vector3 _worldPos = _grid.CellToWorld(spawnPosition) + new Vector3(0.5f, 0, 0);
        carriedObject.SavedData.Position = new SaveData.SimpleVector3(_worldPos);

        IWeighty _spawnedObject = carriedObject.SavedData.InstantiateGameObjectFromSaveData(_impermanent.transform).GetComponent<IWeighty>();
        _spawnedObject.Load(carriedObject.SavedData);
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
