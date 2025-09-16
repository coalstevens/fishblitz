using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWheelBarrow : MonoBehaviour
{
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

    private void OnFacingDirectionChange(CompassDirection curr)
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
            StaticWheelBarrowSelector _staticWheelBarrow = InstantiateStaticWheelBarrow();
            _staticWheelBarrow.SetFacingDirection(_playerMovementController.FacingDirection.Value);
            foreach (Transform child in _facingNorth.transform.parent)
                child.gameObject.SetActive(false);
        }
    }

    private StaticWheelBarrowSelector InstantiateStaticWheelBarrow()
    {
        GameObject _staticWheelBarrowPrefab = Resources.Load<GameObject>("WorldObjects/StaticWheelBarrow");
        if (_staticWheelBarrowPrefab != null)
        {
            Transform _impermanentContainer = GameObject.FindGameObjectWithTag("Impermanent").transform;
            GameObject _staticWheelBarrow = Instantiate(_staticWheelBarrowPrefab, GetActiveWheelBarrowPosition() ,quaternion.identity, _impermanentContainer);
            StaticWheelBarrowSelector _wheelBarrow = _staticWheelBarrow.GetComponent<StaticWheelBarrowSelector>();
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

    private void EnableGameobjectForDirection(CompassDirection direction)
    {
        _facingNorth.SetActive(direction == CompassDirection.North);
        _facingEast.SetActive(direction == CompassDirection.East);
        _facingSouth.SetActive(direction == CompassDirection.South);
        _facingWest.SetActive(direction == CompassDirection.West);
    }

    private Vector3 GetActiveWheelBarrowPosition()
    {
        if (_facingNorth.activeSelf)
            return _facingNorth.transform.position;
        else if (_facingEast.activeSelf)
            return _facingEast.transform.position;
        else if (_facingSouth.activeSelf)
            return _facingSouth.transform.position;
        else if (_facingWest.activeSelf)
            return _facingWest.transform.position;
        else
            throw new Exception("No active wheelbarrow found");
    }
}
