using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCursor : MonoBehaviour
{
    [SerializeField] private Inventory _inventory;
    private Transform _inventoryContainer;
    private Action _unsubscribe;
    
    private void OnEnable()
    {
        _unsubscribe = _inventory.ActiveItemSlot.OnChange(curr => OnActiveItemChange(curr));
        _inventoryContainer = transform.parent.GetChild(1);
    }

    private void OnDisable()
    {
        _unsubscribe();
    }

    private void OnActiveItemChange(int newSlotNum)
    {
        transform.position = _inventoryContainer.GetChild(newSlotNum).position;
    }

}
