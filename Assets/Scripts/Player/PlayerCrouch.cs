using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(PlayerMovement))]
public class PlayerCrouch : MonoBehaviour
{
    private InputAction _crouchAction;
    private PlayerMovement _playerMovementController;

    private void OnEnable()
    {
        PlayerInput _inputController = GetComponent<PlayerInput>();
        _playerMovementController = GetComponent<PlayerMovement>();

        _crouchAction = _inputController.actions["Crouch"];
        Assert.IsNotNull(_crouchAction);

        _crouchAction.started += OnCrouchStarted;
        _crouchAction.canceled += OnCrouchCanceled;
    }

    private void OnDisable()
    {
        _crouchAction.started -= OnCrouchStarted;
        _crouchAction.canceled -= OnCrouchCanceled;
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        if (_playerMovementController.PlayerState.Value == PlayerStates.Crouched)
        {
            _playerMovementController.PlayerState.Value = PlayerStates.Idle;
            return;
        }
    }

    private void OnCrouchStarted(InputAction.CallbackContext context)
    {
        if (_playerMovementController.PlayerState.Value.CanCrouch())
        {
            _playerMovementController.PlayerState.Value = PlayerStates.Crouched;
            return;
        }
    }

}
