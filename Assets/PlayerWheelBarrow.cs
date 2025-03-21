using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWheelBarrow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private GameObject _facingNorth;
    [SerializeField] private GameObject _facingEast;
    [SerializeField] private GameObject _facingSouth;
    [SerializeField] private GameObject _facingWest;
    private PlayerMovementController _playerMovementController;
    List<Action> _unsubscribeHooks = new();
    private PlayerInput _playerInput;

    void OnEnable()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();
        _unsubscribeHooks.Add(_playerData.IsHoldingWheelBarrow.OnChange(curr => OnWheelBarrowingChange(curr)));
        _unsubscribeHooks.Add(_playerMovementController.FacingDirection.OnChange(curr => OnFacingDirectionChange(curr)));
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

    private void OnFacingDirectionChange(FacingDirection curr)
    {
        if (_playerData.IsHoldingWheelBarrow.Value)
            EnableGameobjectForDirection(curr);
    }

    private void OnWheelBarrowingChange(bool isWheelBarrowing)
    {
        if (isWheelBarrowing)
        {
            _playerInput?.SwitchCurrentActionMap("PlayerBarrowing");
            EnableGameobjectForDirection(_playerMovementController.FacingDirection.Value);
        }
        else
        {
            _playerInput?.SwitchCurrentActionMap("Player");
            foreach (Transform child in _facingNorth.transform.parent)
                child.gameObject.SetActive(false);
            StaticWheelBarrow _staticWheelBarrow = InstantiateStaticWheelBarrow();
            _staticWheelBarrow.SetFacingDirection(_playerMovementController.FacingDirection.Value);
        }
    }

    private StaticWheelBarrow InstantiateStaticWheelBarrow()
    {
        GameObject _staticWheelBarrowPrefab = Resources.Load<GameObject>("WorldObjects/StaticWheelBarrow");
        if (_staticWheelBarrowPrefab != null)
        {
            Transform _impermanentContainer = GameObject.FindGameObjectWithTag("Impermanent").transform;
            GameObject _staticWheelBarrow = Instantiate(_staticWheelBarrowPrefab, transform.position, quaternion.identity, _impermanentContainer);
            StaticWheelBarrow _wheelBarrow = _staticWheelBarrow.GetComponent<StaticWheelBarrow>();
            if (_wheelBarrow == null)
                Debug.LogError("Static wheel barrow is missing its wheelbarrow component");
            return _wheelBarrow;
        }
        else
        {
            Debug.LogError("StaticWheelBarrow prefab not found in Resources/WorldObjects");
            return null;
        }
    }

    private void OnDisable()
    {
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

    private void EnableGameobjectForDirection(FacingDirection direction)
    {
        _facingNorth.SetActive(direction == FacingDirection.North);
        _facingEast.SetActive(direction == FacingDirection.East);
        _facingSouth.SetActive(direction == FacingDirection.South);
        _facingWest.SetActive(direction == FacingDirection.West);
    }
}
