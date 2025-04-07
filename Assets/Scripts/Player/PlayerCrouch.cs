using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class PlayerCrouch : MonoBehaviour
{
    private InputAction _crouchAction;
    private PlayerMovementController _playerMovementController;

    void OnEnable()
    {
        PlayerInput _inputController = GetComponent<PlayerInput>();
        Assert.IsNotNull(_inputController);

        _playerMovementController = GetComponent<PlayerMovementController>();
        Assert.IsNotNull(_playerMovementController);

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
        _playerMovementController.PlayerState.Value = PlayerMovementController.PlayerStates.Idle;
    }

    private void OnCrouchStarted(InputAction.CallbackContext context)
    {
        _playerMovementController.PlayerState.Value = PlayerMovementController.PlayerStates.Crouched;
    }
}
