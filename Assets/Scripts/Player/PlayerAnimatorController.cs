using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private PlayerData _playerData;
    private PlayerMovementController _playerMovementController;
    private List<Action> _unsubscribeHooks = new();

    private void OnEnable()
    {
        _animator = GetComponent<Animator>();
        _playerMovementController = GetComponentInParent<PlayerMovementController>();

        _unsubscribeHooks.Add(_playerMovementController.PlayerState.OnChange(curr => OnStateChange(curr)));
        _unsubscribeHooks.Add(_playerMovementController.FacingDirection.OnChange(_ => OnStateChange(_playerMovementController.PlayerState.Value)));
        _unsubscribeHooks.Add(_playerData.IsHoldingWheelBarrow.OnChange(_ => OnStateChange(_playerMovementController.PlayerState.Value)));
        _unsubscribeHooks.Add(_playerData.IsCarrying.OnChange(_ => OnStateChange(_playerMovementController.PlayerState.Value)));
        _unsubscribeHooks.Add(_inventory.ActiveItemSlot.OnChange(_ => OnStateChange(_playerMovementController.PlayerState.Value)));
    }

    private void OnDisable()
    {
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

    private void OnStateChange(PlayerMovementController.PlayerStates curr)
    {
        FacingDirection _facingDir = _playerMovementController.FacingDirection.Value;

        switch (curr)
        {
            case PlayerMovementController.PlayerStates.Idle:
                HandleIdle(_facingDir);
                break;
            case PlayerMovementController.PlayerStates.Walking:
                HandleWalking(_facingDir);
                break;
            case PlayerMovementController.PlayerStates.Fishing:
                HandleFishing(_facingDir);
                break;
            case PlayerMovementController.PlayerStates.Catching:
                HandleCatching(_facingDir);
                break;
            case PlayerMovementController.PlayerStates.Axing:
                HandleChopping(_facingDir);
                break;
            case PlayerMovementController.PlayerStates.Celebrating:
                HandleCelebrating();
                break;
            default:
                break;
        }
    }

    private void HandleCelebrating()
    {
        _animator.Play("Caught");
        Invoke(nameof(SetPlayerIdle), 1.5f);
    }

    private void HandleFishing(FacingDirection facingDir)
    {
        switch (facingDir)
        {
            case FacingDirection.North:
                _animator.Play("N_Fish");
                break;
            case FacingDirection.South:
                _animator.Play("S_Fish");
                break;
            case FacingDirection.East:
                _animator.Play("E_Fish");
                break;
            case FacingDirection.West:
                _animator.Play("W_Fish");
                break;
        }
    }

    private void HandleChopping(FacingDirection facingDir)
    {
        switch (facingDir)
        {
            case FacingDirection.North:
                _animator.Play("N_Chop");
                break;
            case FacingDirection.South:
                _animator.Play("S_Chop");
                break;
            case FacingDirection.East:
                _animator.Play("E_Chop");
                break;
            case FacingDirection.West:
                _animator.Play("W_Chop");
                break;
        }
        Invoke(nameof(SetPlayerIdle), 0.610f);
    }

    private void HandleCatching(FacingDirection facingDir)
    {
        switch (facingDir)
        {
            case FacingDirection.North:
                _animator.Play("N_Catch");
                break;
            case FacingDirection.South:
                _animator.Play("S_Catch");
                break;
            case FacingDirection.East:
                _animator.Play("E_Catch");
                break;
            case FacingDirection.West:
                _animator.Play("W_Catch");
                break;
        }

    }
    private void HandleWalking(FacingDirection facingDir)
    {
        if (_playerData.IsHoldingWheelBarrow.Value)
        {
            HandleBarrowWalking(facingDir);
            return;
        }

        if (_playerData.IsCarrying.Value)
        {
            HandleCarryWalking(facingDir);
            return;
        }

        if (!_inventory.TryGetActiveItemType(out var _activeItem))
        {
            HandleNoToolWalking(facingDir);
            return;
        }

        switch (_activeItem.ItemLabel)
        {
            case "Axe":
                HandleAxeWalking(facingDir);
                break;
            default:
                HandleNoToolWalking(facingDir);
                break;
        }
    }

    private void HandleNoToolWalking(FacingDirection facingDir)
    {
        switch (facingDir)
        {
            case FacingDirection.North:
                _animator.Play("N_Walk", 0, 0.25f);
                break;
            case FacingDirection.South:
                _animator.Play("S_Walk", 0, 0.25f);
                break;
            case FacingDirection.East:
                _animator.Play("E_Walk", 0, 0.25f);
                break;
            case FacingDirection.West:
                _animator.Play("W_Walk", 0, 0.25f);
                break;
        }
    }

    private void HandleAxeWalking(FacingDirection facingDir)
    {
        switch (facingDir)
        {
            case FacingDirection.North:
                _animator.Play("N_AxeWalk", 0, 0.25f);
                break;
            case FacingDirection.South:
                _animator.Play("S_AxeWalk", 0, 0.25f);
                break;
            case FacingDirection.East:
                _animator.Play("E_AxeWalk", 0, 0.25f);
                break;
            case FacingDirection.West:
                _animator.Play("W_AxeWalk", 0, 0.25f);
                break;
        }
    }

    private void HandleBarrowWalking(FacingDirection facingDir)
    {
        switch (facingDir)
        {
            case FacingDirection.North:
                _animator.Play("N_Barrow", 0, 0.25f);
                break;
            case FacingDirection.South:
                _animator.Play("S_Barrow", 0, 0.25f);
                break;
            case FacingDirection.East:
                _animator.Play("E_Barrow", 0, 0.25f);
                break;
            case FacingDirection.West:
                _animator.Play("W_Barrow", 0, 0.25f);
                break;
        }
    }

    private void HandleIdle(FacingDirection facingDir)
    {
        if (_playerData.IsHoldingWheelBarrow.Value)
        {
            HandleBarrowIdle(facingDir);
            return;
        }

        if (_playerData.IsCarrying.Value)
        {
            HandleCarryIdle(facingDir);
            return;
        }

        if (!_inventory.TryGetActiveItemType(out var _activeItem))
        {
            HandleNoToolIdle(facingDir);
            return;
        }

        switch (_activeItem.ItemLabel)
        {
            case "Axe":
                HandleAxeIdle(facingDir);
                break;
            default:
                HandleNoToolIdle(facingDir);
                break;
        }
    }

    private void HandleNoToolIdle(FacingDirection facingDir)
    {
        switch (facingDir)
        {
            case FacingDirection.North:
                _animator.Play("N_Idle");
                break;
            case FacingDirection.South:
                _animator.Play("S_Idle");
                break;
            case FacingDirection.East:
                _animator.Play("E_Idle");
                break;
            case FacingDirection.West:
                _animator.Play("W_Idle");
                break;
        }
    }

    private void HandleAxeIdle(FacingDirection facingDir)
    {
        switch (facingDir)
        {
            case FacingDirection.North:
                _animator.Play("N_AxeIdle");
                break;
            case FacingDirection.South:
                _animator.Play("S_AxeIdle");
                break;
            case FacingDirection.East:
                _animator.Play("E_AxeIdle");
                break;
            case FacingDirection.West:
                _animator.Play("W_AxeIdle");
                break;
        }
    }

    private void HandleCarryIdle(FacingDirection facingDir)
    {
        switch (facingDir)
        {
            case FacingDirection.North:
                _animator.Play("N_CarryIdle");
                break;
            case FacingDirection.South:
                _animator.Play("S_CarryIdle");
                break;
            case FacingDirection.East:
                _animator.Play("E_CarryIdle");
                break;
            case FacingDirection.West:
                _animator.Play("W_CarryIdle");
                break;
        }
    }
    private void HandleCarryWalking(FacingDirection facingDir)
    {
        switch (facingDir)
        {
            case FacingDirection.North:
                _animator.Play("N_Carry");
                break;
            case FacingDirection.South:
                _animator.Play("S_Carry");
                break;
            case FacingDirection.East:
                _animator.Play("E_Carry");
                break;
            case FacingDirection.West:
                _animator.Play("W_Carry");
                break;
        }
    }

    private void HandleBarrowIdle(FacingDirection facingDir)
    {
        switch (facingDir)
        {
            case FacingDirection.North:
                _animator.Play("N_BarrowIdle");
                break;
            case FacingDirection.South:
                _animator.Play("S_BarrowIdle");
                break;
            case FacingDirection.East:
                _animator.Play("E_BarrowIdle");
                break;
            case FacingDirection.West:
                _animator.Play("W_BarrowIdle");
                break;
        }
    }


    private void SetPlayerIdle()
    {
        _playerMovementController.PlayerState.Value = PlayerMovementController.PlayerStates.Idle;
    }
}
