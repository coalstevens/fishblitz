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
        FacingDirections _facingDir = _playerMovementController.FacingDirection.Value;

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

    private void HandleFishing(FacingDirections facingDir)
    {
        switch (facingDir)
        {
            case FacingDirections.North:
                _animator.Play("N_Fish");
                break;
            case FacingDirections.South:
                _animator.Play("S_Fish");
                break;
            case FacingDirections.East:
                _animator.Play("E_Fish");
                break;
            case FacingDirections.West:
                _animator.Play("W_Fish");
                break;
        }
    }
    private void HandleChopping(FacingDirections facingDir)
    {
        switch (facingDir)
        {
            case FacingDirections.North:
                _animator.Play("N_Chop");
                break;
            case FacingDirections.South:
                _animator.Play("S_Chop");
                break;
            case FacingDirections.East:
                _animator.Play("E_Chop");
                break;
            case FacingDirections.West:
                _animator.Play("W_Chop");
                break;
        }
        Invoke(nameof(SetPlayerIdle), 0.610f);
    }

    private void HandleCatching(FacingDirections facingDir) 
    {
        switch (facingDir)
        {
            case FacingDirections.North:
                _animator.Play("N_Catch");
                break;
            case FacingDirections.South:
                _animator.Play("S_Catch");
                break;
            case FacingDirections.East:
                _animator.Play("E_Catch");
                break;
            case FacingDirections.West:
                _animator.Play("W_Catch");
                break;
        }

    }
    private void HandleWalking(FacingDirections facingDir)
    {
        if (_inventory.TryGetActiveItem(out var _activeItem)) {
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
    }
    
    private void HandleNoToolWalking(FacingDirections facingDir)
    {
        switch (facingDir)
        {
            case FacingDirections.North:
                _animator.Play("N_Walk", 0, 0.25f);
                break;
            case FacingDirections.South:
                _animator.Play("S_Walk", 0, 0.25f);
                break;
            case FacingDirections.East:
                _animator.Play("E_Walk", 0, 0.25f);
                break;
            case FacingDirections.West:
                _animator.Play("W_Walk", 0, 0.25f);
                break;
        }
    }

    private void HandleAxeWalking(FacingDirections facingDir)
    {
        switch (facingDir)
        {
            case FacingDirections.North:
                _animator.Play("N_AxeWalk", 0, 0.25f);
                break;
            case FacingDirections.South:
                _animator.Play("S_AxeWalk", 0, 0.25f);
                break;
            case FacingDirections.East:
                _animator.Play("E_AxeWalk", 0, 0.25f);
                break;
            case FacingDirections.West:
                _animator.Play("W_AxeWalk", 0, 0.25f);
                break;
        }
    }
    
    private void HandleIdle(FacingDirections facingDir)
    {
        if (_inventory.TryGetActiveItem(out var _activeItem)) {
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
    }    
    private void HandleNoToolIdle(FacingDirections facingDir)
    {
        switch (facingDir)
        {
            case FacingDirections.North:
                _animator.Play("N_Idle");
                break;
            case FacingDirections.South:
                _animator.Play("S_Idle");
                break;
            case FacingDirections.East:
                _animator.Play("E_Idle");
                break;
            case FacingDirections.West:
                _animator.Play("W_Idle");
                break;
        }
    }
    private void HandleAxeIdle(FacingDirections facingDir)
    {
        switch (facingDir)
        {
            case FacingDirections.North:
                _animator.Play("N_AxeIdle");
                break;
            case FacingDirections.South:
                _animator.Play("S_AxeIdle");
                break;
            case FacingDirections.East:
                _animator.Play("E_AxeIdle");
                break;
            case FacingDirections.West:
                _animator.Play("W_AxeIdle");
                break;
        }
    }

    private void SetPlayerIdle() {
        _playerMovementController.PlayerState.Value = PlayerStates.Idle;
    }
}
