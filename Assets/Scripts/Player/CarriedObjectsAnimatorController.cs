using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// This class could be more efficient, but do i care? no 
public class CarriedObjectsAnimatorController : MonoBehaviour
{
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private WeightyObjectStackData _playerCarriedObjects;
    List<Action> _unsubscribeHooks = new();

    private void OnEnable()
    {
        Assert.IsNotNull(_playerData);
        Assert.IsNotNull(_playerCarriedObjects);
        UpdateStackItemRenderers();
        _unsubscribeHooks.Add(_playerCarriedObjects.StoredObjects.OnChange(_ => UpdateStackItemRenderers()));
    }

    private void OnDisable()
    {
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

    // TODO: Height of carried objects tower... is there a limit? 
    private void UpdateStackItemRenderers()
    {
        Assert.IsTrue(transform.childCount > _playerCarriedObjects.StoredObjects.Count, "There are not enough child objects to display the carried objects");

        int i = 0;
        foreach (StoredWeightyObject carriedObject in _playerCarriedObjects.StoredObjects)
        {
            Transform _child = transform.GetChild(i);
            _child.gameObject.SetActive(true);
            _child.GetComponent<SpriteRenderer>().sprite = carriedObject.Type.NSCarry;
            // _child.GetComponent<SpriteRenderer>().sprite = carriedObject.Type.EWCarry;
            i++;
        }

        for (; i < transform.childCount; i++)
        {
            Transform _child = transform.GetChild(i);
            _child.gameObject.SetActive(false);
        }
    }
}
