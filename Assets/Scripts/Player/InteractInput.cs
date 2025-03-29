using UnityEngine;
using UnityEngine.Assertions;
public class InteractInput : MonoBehaviour
{
    public interface IInteractable
    {
        /// <summary>
        /// Returns false if the object ignores the command.
        /// </summary>
        public bool CursorInteract(Vector3 cursorLocation);
    }

    [SerializeField] private Inventory _inventory;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private GridCursor _gridCursor;
    private PlayerMovementController _playerMovementController;
    private PlayerEnergyManager _playerEnergyManager;
    private PlayerCarry _playerCarry;

    private void OnEnable()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();
        _playerEnergyManager = GetComponent<PlayerEnergyManager>();
        _playerCarry = GetComponent<PlayerCarry>();

        Assert.IsNotNull(_playerMovementController);
        Assert.IsNotNull(_playerEnergyManager);
        Assert.IsNotNull(_playerCarry);
    }

    private void OnInteract()
    {
        // returns if player is not idle or walking
        if (_playerMovementController.PlayerState.Value != PlayerMovementController.PlayerStates.Idle &&
            _playerMovementController.PlayerState.Value != PlayerMovementController.PlayerStates.Walking)
            return;

        // Check for an interactable object
        Vector3Int _cursorLocation = _gridCursor.GridPosition;
        IInteractable _interactable = _gridCursor.FindObjectAtGridCursor<IInteractable>();
        if (_interactable?.CursorInteract(_cursorLocation) == true)
            return;
    } 
}