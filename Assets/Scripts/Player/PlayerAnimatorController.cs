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
        CompassDirection _facingDir = _playerMovementController.FacingDirection.Value;
        if (_facingDir == CompassDirection.SouthEast)
            transform.localScale = new Vector2(1, 1);
        else if (_facingDir == CompassDirection.SouthWest)
            transform.localScale = new Vector2(-1, 1);

        switch (curr)
        {
            case PlayerMovementController.PlayerStates.Idle:
                HandleIdle(_facingDir);
                break;
            case PlayerMovementController.PlayerStates.Running:
                HandleRunning(_facingDir);
                break;
            case PlayerMovementController.PlayerStates.Birding:
                PlayIdleBinos(_facingDir);
                break;
            case PlayerMovementController.PlayerStates.BirdingRunning:
                PlayRunningBinos(_facingDir);
                break;
            case PlayerMovementController.PlayerStates.Fishing:
                throw new NotImplementedException();
                // HandleFishing(_facingDir);
                // break;
            case PlayerMovementController.PlayerStates.Catching:
                throw new NotImplementedException();
                // HandleCatching(_facingDir);
                // break;
            case PlayerMovementController.PlayerStates.Axing:
                throw new NotImplementedException();
                // HandleChopping(_facingDir);
                // break;
            case PlayerMovementController.PlayerStates.Celebrating:
                throw new NotImplementedException();
                // HandleCelebrating();
                // break;
            case PlayerMovementController.PlayerStates.PickingUp:
                throw new NotImplementedException();
                // HandlePickingUp();
                // break;
            case PlayerMovementController.PlayerStates.Crouched:
                throw new NotImplementedException();
                // HandleCrouched(_facingDir);
                // break;
        }
    }

    private void HandleRunning(CompassDirection facingDir)
    {
        if (_playerData.IsHoldingWheelBarrow.Value)
        {
            PlayRunningBarrow(facingDir);
            return;
        }

        if (_playerData.IsCarrying.Value)
        {
            PlayRunningCarry(facingDir);
            return;
        }

        if (!_inventory.TryGetActiveItemType(out var _activeItem))
        {
            PlayRunning(facingDir);
            return;
        }

        switch (_activeItem.ItemLabel)
        {
            case "Axe":
                PlayRunningAxe(facingDir);
                break;
            default:
                PlayRunning(facingDir);
                break;
        }
    }

    private void PlayRunning(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_Running");
                break;
        }
    }

    private void HandleIdle(CompassDirection facingDir)
    {
        // if (_playerData.IsHoldingWheelBarrow.Value)
        // {
        //     HandleBarrowIdle(facingDir);
        //     return;
        // }

        // if (_playerData.IsCarrying.Value)
        // {
        //     HandleCarryIdle(facingDir);
        //     return;
        // }

        if (!_inventory.TryGetActiveItemType(out var _activeItem))
        {
            PlayIdle(facingDir);
            return;
        }

        switch (_activeItem.ItemLabel)
        {
            case "Axe":
                PlayIdleAxe(facingDir);
                break;
            default:
                PlayIdle(facingDir);
                break;
        }
    }

    private void PlayIdle(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_Idle");
                break;
        }
    }

    private void PlayIdleAxe(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_Idle_Axe");
                break;
        }
    }

    private void PlayRunningAxe(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_Running_Axe");
                break;
        }
    }

    private void PlayRunningBinos(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_Running_Binos");
                break;
        }
    }

    private void PlayIdleBinos(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_Idle_Binos");
                break;
        }
    }

    private void PlayRunningBarrow(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_Running_Barrow");
                break;
        }
    }

    private void HandleCarryIdle(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_CarryIdle");
                break;
        }
    }

    private void PlayRunningCarry(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_Running_Carry");
                break;
        }
    }

    private void SetPlayerIdle()
    {
        _playerMovementController.PlayerState.Value = PlayerMovementController.PlayerStates.Idle;
    }

    private void HandleBarrowIdle(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_Idle_Barrow");
                break;
        }
    }

    private void HandleCatching(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.North:
                _animator.Play("N_Catch");
                break;
            case CompassDirection.South:
                _animator.Play("S_Catch");
                break;
            case CompassDirection.East:
                _animator.Play("E_Catch");
                break;
            case CompassDirection.West:
                _animator.Play("W_Catch");
                break;
        }
    }
    
    private void HandleCrouched(CompassDirection facingDir)
    {
        switch (_playerMovementController.FacingDirection.Value)
        {
            case CompassDirection.North:
                _animator.Play("N_Crouch");
                break;
            case CompassDirection.South:
                _animator.Play("S_Crouch");
                break;
            case CompassDirection.East:
                _animator.Play("E_Crouch");
                break;
            case CompassDirection.West:
                _animator.Play("W_Crouch");
                break;
        }
    }

    private void HandleCelebrating()
    {
        _animator.Play("Caught");
        Invoke(nameof(SetPlayerIdle), 1.5f);
    }

    private void HandlePickingUp()
    {
        switch (_playerMovementController.FacingDirection.Value)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_Lifting");
                break;
        }
        Invoke(nameof(SetPlayerIdle), 0.501f);
    }

    private void HandleFishing(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_Fishing");
                break;
        }
    }

    private void HandleChopping(CompassDirection facingDir)
    {
        switch (facingDir)
        {
            case CompassDirection.SouthEast:
            case CompassDirection.SouthWest:
                _animator.Play("S_Chopping");
                break;
        }
        Invoke(nameof(SetPlayerIdle), 0.610f);
    }
}
