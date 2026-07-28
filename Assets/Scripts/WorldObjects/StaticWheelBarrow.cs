using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Animator))]
public class StaticWheelBarrow : MonoBehaviour, IWeightyObjectContainer, UseItemInput.IUsableTarget, BoxData.IBoxPrize
{
    [SerializeField] private WeightyObjectStack _weightyContainer = new();
    [SerializeField] private WeightyObjectStackConfig _stackConfig;

    private PlayerWheelBarrow _wheelBarrow;
    private Animator _animator;
    private PlayerMovement _playerMovementController;
    private StaticWheelBarrowSelector _staticWheelBarrow;
    private PlayerCarry _playerCarry;
    public WeightyObjectStack WeightyStack => _weightyContainer;

    void OnEnable()
    {
        _staticWheelBarrow = GetComponentInParent<StaticWheelBarrowSelector>();
        GameObject _player = GameObject.FindGameObjectWithTag("Player");
        Assert.IsNotNull(_player);

        _playerMovementController = _player.GetComponent<PlayerMovement>();
        _playerCarry = _player.GetComponent<PlayerCarry>();
        _wheelBarrow = _player.GetComponent<PlayerWheelBarrow>();
        _animator = GetComponent<Animator>();

        Assert.IsNotNull(_playerCarry);
        Assert.IsNotNull(_playerMovementController);
        Assert.IsNotNull(_staticWheelBarrow);
        Assert.IsNotNull(_wheelBarrow);
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
        if (IsFacingDirectionForWheelbarrowPickup() &&
            _wheelBarrow.IsHoldingWheelBarrow.Value == false &&
            _playerCarry.IsCarrying.Value == false)
        {
            _wheelBarrow.PickUpStaticWheelbarrow(_weightyContainer);
            _wheelBarrow.IsHoldingWheelBarrow.Value = true;
            Destroy(transform.gameObject);
            return true;
        }
        else
        {
            if (_weightyContainer.IsEmpty())
                return false;
            StoredWeightyObject _storedObject = _weightyContainer.Peek();
            if (_playerCarry.HasEnoughSpace(_storedObject.Type.Weight) == false)
                return false;
            _playerCarry.Push(_weightyContainer.Pop());
            if (_stackConfig != null && _stackConfig.RemoveSound != null)
                PlayerAudioManager.Instance.PlayOneShot(_stackConfig.RemoveSound);
        }
        return false;
    }

    public void AwardPrize()
    {
        if (_animator != null)
            StartCoroutine(PlaySpawnThenIdle());
    }

    private IEnumerator PlaySpawnThenIdle()
    {
        _animator.Play("Spawn");
        yield return new WaitForSeconds(_animator.GetClipLength("Spawn"));
        _animator.Play("Idle");
    }
}
