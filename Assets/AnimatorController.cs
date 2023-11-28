using System;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    private Animator _animator;
    private PlayerMovementController _playerMovementController;

    private void Start()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();
        _animator = GetComponent<Animator>();

        _playerMovementController.CurrState.OnChange((prev, curr) => OnStateChange(curr));
        _playerMovementController.FacingDir.OnChange((prev, curr) => OnStateChange(_playerMovementController.CurrState.Value));
    }

    private void OnStateChange(State curr)
    {
        Direction facingDir = _playerMovementController.FacingDir.Value;

        switch (curr)
        {
            case State.Idle:
                HandleIdle(facingDir);
                return;
            case State.Walking:
                HandleWalking(facingDir);
                return;
            case State.Fishing:
                HandleFishing(facingDir);
                return;
            case State.Catching:
                HandleCatching(facingDir);
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
        switch (facingDir)
        {
            case Direction.Up:
                _animator.Play("N_Walk", 0, 2);
                return;
            case Direction.Down:
                _animator.Play("S_Walk", 0, 2);
                return;
            case Direction.Right:
                _animator.Play("E_Walk", 0, 2);
                return;
            case Direction.Left:
                _animator.Play("W_Walk", 0, 2);
                return;
        }
    }

    private void HandleIdle(Direction facingDir)
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
}
