using System;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.Assertions;

// TODO: There are gonna be some issues with overlapping cover colliders

public class PlayerHurtbox : MonoBehaviour, IHurtBox
{
    public Reactive<bool> IsCovered = new Reactive<bool>(false);
    [SerializeField] private Reactive<bool> _isInCoveredArea = new Reactive<bool>(false);
    [SerializeField] private Reactive<bool> _isCrouched = new Reactive<bool>(false);
    private PlayerMovementController _playerMovementController;
    private Transform _player;
    private List<Action> _unsubscribeCBs = new();

    private void OnEnable()
    {
        _player = transform.parent;
        Assert.IsNotNull(_player);
        _playerMovementController = _player.GetComponent<PlayerMovementController>();
        Assert.IsNotNull(_playerMovementController);

        _unsubscribeCBs.Add(_playerMovementController.PlayerState.OnChange(curr => CheckIfCrouched(curr)));
        _unsubscribeCBs.Add(_isCrouched.OnChange(_ => CheckIfCovered()));
        _unsubscribeCBs.Add(_isInCoveredArea.OnChange(_ => CheckIfCovered()));
    }

    private void OnDisable()
    {
        foreach (var cb in _unsubscribeCBs)
            cb();
        _unsubscribeCBs.Clear();
    }

    private void CheckIfCovered()
    {
        IsCovered.Value = _isInCoveredArea.Value && _isCrouched.Value;
    }

    private void CheckIfCrouched(PlayerMovementController.PlayerStates curr)
    {
        _isCrouched.Value = curr == PlayerMovementController.PlayerStates.Crouched;
    }

    public void TakeDamage()
    {
        throw new NotImplementedException();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Cover"))
        {
            _isInCoveredArea.Value = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Cover"))
        {
            _isInCoveredArea.Value = false;
        }
    }
}
