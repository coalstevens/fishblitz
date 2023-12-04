using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ItemCursorController : MonoBehaviour
{
    private GameObject _inventoryContainer;
    private GameObject _itemCursor;
    public GameObject ActiveItem;
    
    void Start()
    {
        _inventoryContainer = GameObject.FindGameObjectWithTag("InventoryContainer");
        _itemCursor = GameObject.FindGameObjectWithTag("ItemCursor");
        ActiveItem = selectAndGetItem(0);
    }
    void OnItemSelect1() {
        ActiveItem = selectAndGetItem(0);
    }
    void OnItemSelect2() {
        ActiveItem = selectAndGetItem(1);
    }
    void OnItemSelect3() {
        ActiveItem = selectAndGetItem(2);
    }
    void OnItemSelect4() {
        ActiveItem = selectAndGetItem(3);
    }
    void OnItemSelect5() {
        ActiveItem = selectAndGetItem(4);
    }
    void OnItemSelect6() {
        ActiveItem = selectAndGetItem(5);
    }
    void OnItemSelect7() {
        ActiveItem = selectAndGetItem(6);
    }
    void OnItemSelect8() {
        ActiveItem = selectAndGetItem(7);
    }
    void OnItemSelect9() {
        ActiveItem = selectAndGetItem(8);
    }
    void OnItemSelect10() {
        ActiveItem = selectAndGetItem(9);
    }

    GameObject selectAndGetItem(int slotIndex) {
        Transform itemSlotToSelect = _inventoryContainer.transform.GetChild(slotIndex);
        _itemCursor.transform.position = itemSlotToSelect.position;
        
        if (itemSlotToSelect.childCount > 0)
        {
            return itemSlotToSelect.gameObject.GetComponent<ItemSlot>().SlotItem;
        }
        return null;
    }
}
