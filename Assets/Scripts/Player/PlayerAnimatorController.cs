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
        
        _playerMovementController.CurrState.OnChange((prev, curr) => OnStateChange(curr));
        _playerMovementController.FacingDir.OnChange((prev, curr) => OnStateChange(_playerMovementController.CurrState.Value));
        _inventory.ActiveItemSlot.OnChange((prev, curr) => OnStateChange(_playerMovementController.CurrState.Value));
    }

    private void OnStateChange(State curr)
    {
        Direction _facingDir = _playerMovementController.FacingDir.Value;

        switch (curr)
        {
            case State.Idle:
                HandleIdle(_facingDir);
                return;
            case State.Walking:  
                HandleWalking(_facingDir);
                return;
            case State.Fishing:
                HandleFishing(_facingDir);
                return;
            case State.Catching:
                HandleCatching(_facingDir);
                return;
            case State.Celebrating:
                HandleCelebrating();
                return;
            default:
                return;
        }
    }

    private void HandleCelebrating()
    {
        _animator.Play("Caught");
    }

    private void HandleFishing(Direction facingDir)
    {
        switch (facingDir)
        {
            case Direction.Up:
                _animator.Play("N_Fish");
                return;
            case Direction.Down:
                _animator.Play("S_Fish");
                return;
            case Direction.Right:
                _animator.Play("E_Fish");
                return;
            case Direction.Left:
                _animator.Play("W_Fish");
                return;
        }
    }

    private void HandleCatching(Direction facingDir) 
    {
        switch (facingDir)
        {
            case Direction.Up:
                _animator.Play("N_Catch");
                return;
            case Direction.Down:
                _animator.Play("S_Catch");
                return;
            case Direction.Right:
                _animator.Play("E_Catch");
                return;
            case Direction.Left:
                _animator.Play("W_Catch");
                return;
        }

    }
    private void HandleWalking(Direction facingDir)
    {
        if (_inventory.TryGetActiveItem(out var _activeItem)) {
            switch (_activeItem.ItemName)
            {
                case "Axe":
                    HandleAxeWalking(facingDir);
                    return;
                default:
                    break;
            }
        }
        HandleNoToolWalking(facingDir);
    }
    
    private void HandleNoToolWalking(Direction facingDir)
    {
        switch (facingDir)
        {
            case Direction.Up:
                _animator.Play("N_Walk", 0, 0.25f);
                return;
            case Direction.Down:
                _animator.Play("S_Walk", 0, 0.25f);
                return;
            case Direction.Right:
                _animator.Play("E_Walk", 0, 0.25f);
                return;
            case Direction.Left:
                _animator.Play("W_Walk", 0, 0.25f);
                return;
        }
    }

    private void HandleAxeWalking(Direction facingDir)
    {
        switch (facingDir)
        {
            case Direction.Up:
                _animator.Play("N_AxeWalk", 0, 0.25f);
                return;
            case Direction.Down:
                _animator.Play("S_AxeWalk", 0, 0.25f);
                return;
            case Direction.Right:
                _animator.Play("E_AxeWalk", 0, 0.25f);
                return;
            case Direction.Left:
                _animator.Play("W_AxeWalk", 0, 0.25f);
                return;
        }
    }
    
    private void HandleIdle(Direction facingDir)
    {
        if (_inventory.TryGetActiveItem(out var _activeItem)) {
            switch (_activeItem.ItemName)
            {
                case "Axe":
                    HandleAxeIdle(facingDir);
                    return;
                default:
                    break;
            }
        }
        HandleNoToolIdle(facingDir);
    }    
    private void HandleNoToolIdle(Direction facingDir)
    {
        switch (facingDir)
        {
            case Direction.Up:
                _animator.Play("N_Idle");
                return;
            case Direction.Down:
                _animator.Play("S_Idle");
                return;
            case Direction.Right:
                _animator.Play("E_Idle");
                return;
            case Direction.Left:
                _animator.Play("W_Idle");
                return;
        }
    }
    private void HandleAxeIdle(Direction facingDir)
    {
        switch (facingDir)
        {
            case Direction.Up:
                _animator.Play("N_AxeIdle");
                return;
            case Direction.Down:
                _animator.Play("S_AxeIdle");
                return;
            case Direction.Right:
                _animator.Play("E_AxeIdle");
                return;
            case Direction.Left:
                _animator.Play("W_AxeIdle");
                return;
        }
    }
}
