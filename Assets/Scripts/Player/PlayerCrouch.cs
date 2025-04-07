using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class PlayerCrouch : MonoBehaviour
{
    [SerializeField] private Transform _playerSpriteContainer;
    [SerializeField] private PlayerHurtbox _playerHurtbox;
    [SerializeField] Color _coveredColor; 
    private InputAction _crouchAction;
    private PlayerMovementController _playerMovementController;
    private List<SpriteRenderer> _playerRenderers = new List<SpriteRenderer>();
    private List<Action> _unsubscribeCBs = new();

    private void OnEnable()
    {
        PlayerInput _inputController = GetComponent<PlayerInput>();
        Assert.IsNotNull(_inputController);

        _playerMovementController = GetComponent<PlayerMovementController>();
        Assert.IsNotNull(_playerMovementController);

        _crouchAction = _inputController.actions["Crouch"];
        Assert.IsNotNull(_crouchAction);

        Assert.IsNotNull(_playerSpriteContainer);
        Assert.IsNotNull(_playerHurtbox);

        _playerRenderers = _playerSpriteContainer.GetComponentsInChildren<SpriteRenderer>().ToList();
        _unsubscribeCBs.Add(_playerHurtbox.IsCovered.OnChange(curr => ApplyTintToPlayer(curr)));
        _crouchAction.started += OnCrouchStarted;
        _crouchAction.canceled += OnCrouchCanceled;
    }

    private void OnDisable()
    {
        foreach (var cb in _unsubscribeCBs)
            cb();
        _unsubscribeCBs.Clear();
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
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Walking)
        {
            _playerMovementController.PlayerState.Value = PlayerMovementController.PlayerStates.Crouched;
            return;
        }
    }

    private void ApplyTintToPlayer(bool isTinted)
    {
        if (isTinted)
        {
            foreach (var renderer in _playerRenderers)
            {
                renderer.color *= _coveredColor;
            }
        }
        else
        {
            foreach (var renderer in _playerRenderers)
            {
                renderer.color = new Color(renderer.color.r / _coveredColor.r,
                    renderer.color.g / _coveredColor.g,
                    renderer.color.b / _coveredColor.b);
            }
        }

    }
}
