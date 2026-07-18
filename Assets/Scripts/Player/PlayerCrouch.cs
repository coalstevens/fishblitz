using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(PlayerMovementController))]
public class PlayerCrouch : MonoBehaviour
{
    private InputAction _crouchAction;
    private PlayerMovementController _playerMovementController;

    private void OnEnable()
    {
        PlayerInput _inputController = GetComponent<PlayerInput>();
        _playerMovementController = GetComponent<PlayerMovementController>();

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
        if (_playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Crouched)
        {
            _playerMovementController.PlayerState.Value = PlayerMovementController.PlayerStates.Idle;
            return;
        }
    }

    private void OnCrouchStarted(InputAction.CallbackContext context)
    {
        if (_playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Idle ||
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Running)
        {
            _playerMovementController.PlayerState.Value = PlayerMovementController.PlayerStates.Crouched;
            return;
        }
    }

}
