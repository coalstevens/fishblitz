using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWheelBarrow : MonoBehaviour
{
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private GameObject _playerWheelBarrow;
    [SerializeField] private GameObject _staticWheelBarrowPrefab;
    List<Action> _unsubscribeHooks = new();
    private PlayerInput _playerInput;
    private Rigidbody2D _rb;

    void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>();
        _unsubscribeHooks.Add(_playerData.IsHoldingWheelBarrow.OnChange(curr => OnWheelBarrowingChange(curr)));
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerInput = player.GetComponent<PlayerInput>();
    }

    private void OnUseWheelBarrow()
    {
        // not sure what to use this for yet. maybe dumping the wheelbarrow? or looking at its contents 
    }

    private void OnReleaseWheelBarrow()
    {
        _playerData.IsHoldingWheelBarrow.Value = false;
    }


    private void OnWheelBarrowingChange(bool isWheelBarrowing)
    {
        if (isWheelBarrowing)
        {
            Debug.Log($"Player position change {Time.frameCount}");
            _playerInput?.SwitchCurrentActionMap("PlayerBarrowing");
        }
        else
        {
            _playerInput?.SwitchCurrentActionMap("Player");
            StaticWheelBarrowSelector _staticWheelBarrow = InstantiateStaticWheelBarrow();
            _staticWheelBarrow.SetFacingDirection(PlayerAnimatorController.Instance.AnimationDirection);
        }
    }

    private StaticWheelBarrowSelector InstantiateStaticWheelBarrow()
    {
        if (_staticWheelBarrowPrefab != null)
        {
            Transform impermanentContainer = GameObject.FindGameObjectWithTag("Impermanent").transform;
            GameObject newStaticWheelBarrow = Instantiate(
                _staticWheelBarrowPrefab, 
                _playerWheelBarrow.transform.position,
                quaternion.identity, 
                impermanentContainer);
            StaticWheelBarrowSelector newBarrow = newStaticWheelBarrow.GetComponent<StaticWheelBarrowSelector>();
            if (_playerWheelBarrow == null)
                Debug.LogError("Static wheel barrow is missing its wheelbarrow component");
            return newBarrow;
        }
        else
        {
            Debug.LogError("StaticWheelBarrow prefab not set in inspector.");
            return null;
        }
    }

    private void OnDisable()
    {
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

}
