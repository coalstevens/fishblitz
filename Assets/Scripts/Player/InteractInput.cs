using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerEnergyManager), typeof(PlayerCarry))]
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
    [SerializeField] private GridCursor _gridCursor;
    [SerializeField] private Logger _logger = new();
    private PlayerMovement _playerMovementController;
    private PlayerEnergyManager _playerEnergyManager;
    private PlayerCarry _playerCarry;

    private void OnEnable()
    {
        _playerMovementController = GetComponent<PlayerMovement>();
        _playerEnergyManager = GetComponent<PlayerEnergyManager>();
        _playerCarry = GetComponent<PlayerCarry>();
    }

    private void OnInteract()
    {
        // returns if player is not idle or walking
        if (_playerMovementController.PlayerState.Value != PlayerMovement.PlayerStates.Idle &&
            _playerMovementController.PlayerState.Value != PlayerMovement.PlayerStates.Running)
        {
            _logger.Info("Attempted to interact but player state does not allow.");
            return;
        }

        // Check for an interactable object
        Vector3Int cursorLocation = _gridCursor.GridPosition;
        IInteractable interactable = _gridCursor.FindObjectAtGridCursor<IInteractable>();
        if (interactable == null)
        {
            _logger.Info("Attempted to interact but there is no item under cursor.");
        }
        string name = interactable is MonoBehaviour mb ? mb.gameObject.name : "";

        // Player can't interact except for weighty interactables
        if (_playerCarry.IsCarrying.Value && interactable is not IWeighty && interactable is not IWeightyObjectContainer)
        {
            _logger.Info($"Can't interact with {name} while carrying.");
            return;
        }

        if (interactable?.CursorInteract(cursorLocation) == true)
        {
            _logger.Info($"Interacting with {name}");
            return;
        }
    }
}