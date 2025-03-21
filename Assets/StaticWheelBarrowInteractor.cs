using UnityEngine;

public class StaticWheelBarrowInteractor : MonoBehaviour, PlayerInteractionManager.IInteractable
{
    [SerializeField] private PlayerData _playerData;
    private PlayerMovementController _playerMovementController;
    private StaticWheelBarrow _staticWheelBarrow;

    void OnEnable()
    {
        _staticWheelBarrow = GetComponentInParent<StaticWheelBarrow>();
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
    }

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_playerMovementController.FacingDirection.Value == _staticWheelBarrow.FacingDirection &&
            _playerData.IsHoldingWheelBarrow.Value == false)
        {
            _playerData.IsHoldingWheelBarrow.Value = true;
            Destroy(transform.parent.gameObject);
            return true;
        }
        return false;
    }
}
