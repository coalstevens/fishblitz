using System.Collections;
using UnityEngine;
public class ItemCursorController : MonoBehaviour
{
    private Inventory _inventory;
    private Transform _inventoryContainer;
    private GameObject _itemCursor;
    
    private void Awake() {
        _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        _inventoryContainer = GameObject.FindGameObjectWithTag("InventoryContainer").transform;
        _itemCursor = GameObject.FindGameObjectWithTag("ItemCursor");
        StartCoroutine(WaitForUIUpdate());
    }

    private IEnumerator WaitForUIUpdate() {
        yield return null;
        SetActiveSlot(0);
    }

    private void OnItemSelect1() {
        SetActiveSlot(0);
    }
    private void OnItemSelect2() {
        SetActiveSlot(1);
    }
    private void OnItemSelect3() {
        SetActiveSlot(2);
    }
    private void OnItemSelect4() {
        SetActiveSlot(3);
    }
    private void OnItemSelect5() {
        SetActiveSlot(4);
    }
    private void OnItemSelect6() {
        SetActiveSlot(5);
    }
    private void OnItemSelect7() {
        SetActiveSlot(6);
    }
    private void OnItemSelect8() {
        SetActiveSlot(7);
    }
    private void OnItemSelect9() {
        SetActiveSlot(8);
    }
    private void OnItemSelect10() {
        SetActiveSlot(9);
    }

    public void SetActiveSlot(int slotIndex) {
        Transform _itemSlotToBeActive = _inventoryContainer.transform.GetChild(slotIndex);
        _inventory.ActiveItemSlot.Value = slotIndex;
        _itemCursor.transform.position = _itemSlotToBeActive.position;
    }
}
