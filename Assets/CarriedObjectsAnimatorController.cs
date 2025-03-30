using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// This class could be more efficient, but do i care? no 
public class CarriedObjectsAnimatorController : MonoBehaviour
{
    [SerializeField] private GameObject _facingNorth;
    [SerializeField] private GameObject _facingEast;
    [SerializeField] private GameObject _facingSouth;
    [SerializeField] private GameObject _facingWest;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private WeightyObjectStackData _playerCarriedObjects;
    List<Action> _unsubscribeHooks = new();
    private PlayerMovementController _playerMovementController;
    private Dictionary<FacingDirection, GameObject> _facingObjects;

    private void OnEnable()
    {
        _facingObjects = new() {
            { FacingDirection.North, _facingNorth },
            { FacingDirection.East, _facingEast },
            { FacingDirection.South, _facingSouth },
            { FacingDirection.West, _facingWest }
        };

        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
        Assert.IsNotNull(_playerMovementController);
        Assert.IsNotNull(_facingNorth);
        Assert.IsNotNull(_facingEast);
        Assert.IsNotNull(_facingSouth);
        Assert.IsNotNull(_facingWest);
        Assert.IsNotNull(_playerData);
        Assert.IsNotNull(_playerCarriedObjects);
        _unsubscribeHooks.Add(_playerData.IsCarrying.OnChange(curr => OnCarryingChange(curr)));
        _unsubscribeHooks.Add(_playerCarriedObjects.StoredObjects.OnChange(_ => UpdateStackItemRenderers()));
        _unsubscribeHooks.Add(_playerMovementController.FacingDirection.OnChange(curr => OnFacingDirectionChange(curr)));
    }

    private void OnDisable()
    {
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

    private void OnFacingDirectionChange(FacingDirection curr)
    {
        if (_playerData.IsCarrying.Value)
            EnableGameobjectForDirection(curr);
    }

    private void OnCarryingChange(bool isCarrying)
    {
        if (isCarrying)
        {
            EnableGameobjectForDirection(_playerMovementController.FacingDirection.Value);
        }
        else
        {
            foreach (Transform child in _facingNorth.transform.parent)
                child.gameObject.SetActive(false);
        }
    }

    private void UpdateStackItemRenderers()
    {
        FacingDirection _currentDirection = _playerMovementController.FacingDirection.Value;
        Transform _activeObject = _facingObjects[_currentDirection].transform;
        int i = 0;

        foreach (StoredWeightyObject _carriedObject in _playerCarriedObjects.StoredObjects)
        {
            Transform _child = _activeObject.GetChild(i);
            _child.gameObject.SetActive(true);
            if (_currentDirection == FacingDirection.North || _currentDirection == FacingDirection.South)
                _child.GetComponent<SpriteRenderer>().sprite = _carriedObject.Type.NSCarry;
            else
                _child.GetComponent<SpriteRenderer>().sprite = _carriedObject.Type.EWCarry;
            i++;
        }

        for (; i < _activeObject.childCount; i++)
        {
            Transform _child = _activeObject.GetChild(i);
            _child.gameObject.SetActive(false);
        }
    }

    private void EnableGameobjectForDirection(FacingDirection direction)
    {
        _facingNorth.SetActive(direction == FacingDirection.North);
        _facingEast.SetActive(direction == FacingDirection.East);
        _facingSouth.SetActive(direction == FacingDirection.South);
        _facingWest.SetActive(direction == FacingDirection.West);
        UpdateStackItemRenderers();
    }
}
