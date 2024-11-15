using System;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    private Animator _animator;
    private PlayerMovementController _playerMovementController;
    private Inventory _inventory;

    private void Start()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();
        _animator = GetComponent<Animator>();
        _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        
        _playerMovementController.PlayerState.OnChange((prev, curr) => OnStateChange(curr));
        _playerMovementController.FacingDirection.OnChange((prev, curr) => OnStateChange(_playerMovementController.PlayerState.Value));
        _inventory.ActiveItemSlot.OnChange((prev, curr) => OnStateChange(_playerMovementController.PlayerState.Value));
    }

    private void OnStateChange(PlayerStates curr)
    {
        FacingDirection _facingDir = _playerMovementController.FacingDirection.Value;

        switch (curr)
        {
            case PlayerStates.Idle:
                HandleIdle(_facingDir);
                break;
            case PlayerStates.Walking:  
                HandleWalking(_facingDir);
                break;
            case PlayerStates.Fishing:
                HandleFishing(_facingDir);
                break;
            case PlayerStates.Catching:
                HandleCatching(_facingDir);
                break;
            case PlayerStates.Axing:
                HandleChopping(_facingDir);
                break;
            case PlayerStates.Celebrating:
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
        // Active item null
        if (!_inventory.TryGetActiveItem(out var _activeItem)) {
            HandleNoToolWalking(facingDir);
            return;
        }

        // Active item switch
        switch (_activeItem.ItemName)
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
    
    private void HandleIdle(FacingDirection facingDir)
    {
        // Active item null
        if (!_inventory.TryGetActiveItem(out var _activeItem)) {
            HandleNoToolIdle(facingDir);
            return;
        }
        
        // Active item switch
        switch (_activeItem.ItemName)
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

    private void SetPlayerIdle() {
        _playerMovementController.PlayerState.Value = PlayerStates.Idle;
    }
}
