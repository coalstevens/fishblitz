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

    public bool CursorInteract(Vector3 cursorLocation)
    {
        // pick up wheelbarrow
        if (_playerMovementController.FacingDirection.Value == _staticWheelBarrow.FacingDirection &&
            _playerData.IsHoldingWheelBarrow.Value == false)
        {
            _playerData.IsHoldingWheelBarrow.Value = true;
            Destroy(transform.parent.gameObject);
            return true;
        }
        else // try to take from wheelbarrow
        {
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
