using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerCrouch : MonoBehaviour
{
    private InputAction _crouchAction;

    private void OnEnable()
    {
        PlayerInput _inputController = GetComponent<PlayerInput>();

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
        if (PlayerStateController.Instance.PlayerState.Value == PlayerStates.Crouched)
        {
            PlayerStateController.Instance.TransitionTo(PlayerStates.Default);
            return;
        }
    }

    private void OnCrouchStarted(InputAction.CallbackContext context)
    {
        if (PlayerStateController.Instance.PlayerState.Value.CanCrouch())
        {
            PlayerStateController.Instance.TransitionTo(PlayerStates.Crouched);
            return;
        }
    }

}
