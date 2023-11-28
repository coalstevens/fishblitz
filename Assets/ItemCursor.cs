using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ItemCursor : MonoBehaviour
{
    private GameObject _inventoryContainer;
    private GameObject _itemCursor;
    public GameObject ActiveItem;
    
    void Start()
    {
        _inventoryContainer = GameObject.FindGameObjectWithTag("InventoryContainer");
        _itemCursor = GameObject.FindGameObjectWithTag("ItemCursor");
        ActiveItem = selectAndGetItem(0);
        ActiveItem = selectAndGetItem(0);
    }
    void OnItemSelect1() {
        ActiveItem = selectAndGetItem(0);
    }
    void OnItemSelect2() {
        ActiveItem = selectAndGetItem(1);
    }

    GameObject selectAndGetItem(int slotIndex) {
        Transform itemSlotToSelect = _inventoryContainer.transform.GetChild(slotIndex);
        UnityEngine.Debug.Log(_itemCursor.transform.position);
        UnityEngine.Debug.Log(itemSlotToSelect.position);
        _itemCursor.transform.position = itemSlotToSelect.position;
        UnityEngine.Debug.Log(_itemCursor.transform.position);
        UnityEngine.Debug.Log(itemSlotToSelect.position);

        if (itemSlotToSelect.childCount > 0)
        {
            return itemSlotToSelect.gameObject.GetComponent<ItemSlot>().SlotItem;
        }
        return null;
    }
}
