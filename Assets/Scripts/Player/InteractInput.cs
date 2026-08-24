using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerEnergyManager))]
[RequireComponent(typeof(PlayerCarry))]
[RequireComponent(typeof(PlayerInteraction))]
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
    [SerializeField] private Logger _logger = new();
    private PlayerInteraction _playerInteraction;
    private PlayerMovement _playerMovementController;
    private PlayerEnergyManager _playerEnergyManager;
    private PlayerCarry _playerCarry;

    private void OnEnable()
    {
        _playerMovementController = GetComponent<PlayerMovement>();
        _playerEnergyManager = GetComponent<PlayerEnergyManager>();
        _playerCarry = GetComponent<PlayerCarry>();
        _playerInteraction = GetComponent<PlayerInteraction>();
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
        IInteractable interactable = _playerInteraction.FindTarget<IInteractable>();
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

        if (interactable?.CursorInteract(_playerInteraction.ResolvePoint) == true)
        {
            _logger.Info($"Interacting with {name}");
            return;
        }
    }
}