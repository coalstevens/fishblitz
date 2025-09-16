using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class WheelBarrowContainedRenderers : MonoBehaviour
{
    [SerializeField] private WeightyObjectStackData _wheelBarrowStack;
    [SerializeField] private CompassDirection _facingDirection = CompassDirection.North;
    List<Action> _unsubscribeHooks = new();

    private void OnEnable()
    {
        Assert.IsNotNull(_wheelBarrowStack);
        _unsubscribeHooks.Add(_wheelBarrowStack.StoredObjects.OnChange(_ => UpdateContainedRenderers()));
        UpdateContainedRenderers();
    }

    private void OnDisable()
    {
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

    private void UpdateContainedRenderers()
    {
        Assert.IsTrue(transform.childCount > _wheelBarrowStack.StoredObjects.Count, "There are not enough child objects to display the contained objects");

        int i = 0;
        foreach (StoredWeightyObject _carriedObject in _wheelBarrowStack.StoredObjects)
        {
            Transform _child = transform.GetChild(i);
            _child.gameObject.SetActive(true);
            if (_facingDirection == CompassDirection.North || _facingDirection == CompassDirection.South)
                _child.GetComponent<SpriteRenderer>().sprite = _carriedObject.Type.NSCarry;
            else
                _child.GetComponent<SpriteRenderer>().sprite = _carriedObject.Type.EWCarry;
            i++;
        }

        for (; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
