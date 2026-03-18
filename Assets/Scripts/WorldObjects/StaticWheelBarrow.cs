using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class StaticWheelBarrow : MonoBehaviour, IWeightyObjectContainer, UseItemInput.IUsableTarget
{
    [SerializeField] private PlayerData _playerData;
    private PlayerMovementController _playerMovementController;
    private StaticWheelBarrowSelector _staticWheelBarrow;
    private WeightyObjectStack _weightyContainer;
    private PlayerCarry _playerCarry;
    public WeightyObjectStack WeightyStack => _weightyContainer;

    void OnEnable()
    {
        _staticWheelBarrow = GetComponentInParent<StaticWheelBarrowSelector>();
        GameObject _player = GameObject.FindGameObjectWithTag("Player");
        Assert.IsNotNull(_player);

        _playerMovementController = _player.GetComponent<PlayerMovementController>();
        _playerCarry = _player.GetComponent<PlayerCarry>();
        _weightyContainer = GetComponent<WeightyObjectStack>();

        Assert.IsNotNull(_playerCarry);
        Assert.IsNotNull(_weightyContainer);
        Assert.IsNotNull(_playerMovementController);
        Assert.IsNotNull(_staticWheelBarrow);
        Assert.IsNotNull(_playerData);
    }
    private bool IsFacingDirectionForWheelbarrowPickup()
    {
        CompassDirection playerDirection = _playerMovementController.Direction.Value;

        CompassDirection[] acceptedDirections = _staticWheelBarrow.FacingDirection switch
        {
            CompassDirection.SouthEast => new[]
            {
            CompassDirection.East,
            CompassDirection.SouthEast,
            CompassDirection.NorthEast
        },

            CompassDirection.SouthWest => new[]
            {
            CompassDirection.West,
            CompassDirection.SouthWest,
            CompassDirection.SouthEast
        },

            _ => null
        };

        if (acceptedDirections == null)
        {
            Debug.LogError("Wheelbarrow direction not handled");
            return false;
        }

        return acceptedDirections.Contains(playerDirection);
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        Debug.Log("Interacted w/ wheelbarrow");
        // pick up wheelbarrow
        Debug.Log($" Player Direction {_playerMovementController.Direction.Value}");
        Debug.Log($" Barrow Direction {_staticWheelBarrow.FacingDirection}");
        Debug.Log($" Is Holding Barrow {_playerData.IsHoldingWheelBarrow}");
        Debug.Log($" Is Carrying {_playerData.IsCarrying.Value}");
        if (IsFacingDirectionForWheelbarrowPickup() &&
            _playerData.IsHoldingWheelBarrow.Value == false &&
            _playerData.IsCarrying.Value == false)
        {
            Debug.Log("Tried to take wheelbarrow");
            Debug.Log($"Wheelbarrow aquired {Time.frameCount}");
            _playerData.IsHoldingWheelBarrow.Value = true;
            Destroy(transform.gameObject);
            return true;
        }
        else // try to take from wheelbarrow
        {
            Debug.Log("Tried to take item from wheelbarrow");
            if (_weightyContainer.IsEmpty())
                return false;
            StoredWeightyObject _storedObject = _weightyContainer.Peek();
            if (_playerCarry.HasEnoughSpace(_storedObject.Type.Weight) == false)
                return false;
            _playerCarry.Push(_weightyContainer.Pop());
        }
        return false;
    }
}
