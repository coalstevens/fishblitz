using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ItemCursorController : MonoBehaviour
{
    private Inventory _inventory;
    private Transform _inventoryContainer;
    private GameObject _itemCursor;
    
    void Start()
    {
        _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        _inventoryContainer = GameObject.FindGameObjectWithTag("InventoryContainer").transform;
        _itemCursor = GameObject.FindGameObjectWithTag("ItemCursor");
        SetActiveSlot(0);
    }

    void OnItemSelect1() {
        SetActiveSlot(0);
    }
    void OnItemSelect2() {
        SetActiveSlot(1);
    }
    void OnItemSelect3() {
        SetActiveSlot(2);
    }
    void OnItemSelect4() {
        SetActiveSlot(3);
    }
    void OnItemSelect5() {
        SetActiveSlot(4);
    }
    void OnItemSelect6() {
        SetActiveSlot(5);
    }
    void OnItemSelect7() {
        SetActiveSlot(6);
    }
    void OnItemSelect8() {
        SetActiveSlot(7);
    }
    void OnItemSelect9() {
        SetActiveSlot(8);
    }
    void OnItemSelect10() {
        SetActiveSlot(9);
    }

    void SetActiveSlot(int slotIndex) {
        Transform _itemSlotToBeActive = _inventoryContainer.transform.GetChild(slotIndex);
        _inventory.ActiveItemSlot.Value = slotIndex;
        _itemCursor.transform.position = _itemSlotToBeActive.position;
    }
}
