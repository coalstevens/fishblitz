using System;
using UnityEngine;

public class ItemCursor : MonoBehaviour
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Transform _inventoryContainer;
    private Action _unsubscribe;
    
    private void OnEnable()
    {
        _unsubscribe = _inventory.ActiveItemSlot.OnChange(curr => OnActiveItemChange(curr));
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
