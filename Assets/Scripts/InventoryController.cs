using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject _inventoryContainer;
    private GameObject _itemCursorController;
    void Start()
    {
        _inventoryContainer = GameObject.FindGameObjectWithTag("InventoryContainer");
        _itemCursorController = GameObject.FindGameObjectWithTag("ItemCursorController");
    }

    public GameObject GetActiveItem() 
    {
        return _itemCursorController.GetComponent<ItemCursorController>().ActiveItem;
    }

    // Adds quantity to existing item or creates a new item
    // returns true on success
    // return false if inventory full
    public bool AddItem(string itemName, int quantity) {
        Item currentItem;
        int residual = quantity;

        if (quantity == 0) {
            return true;
        }

        foreach (Transform slot in _inventoryContainer.transform) {
            // empty item slot
            if (slot.childCount == 0) {
                continue;
            }
            
            currentItem = slot.GetChild(0).GetComponent<Item>();
            // not the item
            if (currentItem.Name != itemName) {
                continue;
            }

            // item has enough capacity
            if (residual + currentItem.Quantity <= currentItem.Capacity) {
                currentItem.Quantity += residual;
                return true;
            }

            // increases item quantity to max and calculates residual 
            if (residual + currentItem.Quantity > currentItem.Capacity) {
                currentItem.Quantity = currentItem.Capacity;
                residual = residual + currentItem.Quantity - currentItem.Capacity;  
            }
        }

        // loop above only completes if residual > 0
        return AddItemAsNewObject(itemName, residual);
    }

    // Removes a quantity of an item from inventory
    public bool RemoveItem(string itemName, int quantity) {
        List<Item> foundItems = new List<Item>();
        Item currentItem;
        Transform firstChild;
        int totalQuantity = 0;
        int residual;

        foreach (Transform child in _inventoryContainer.transform) {
            // empty item slot
            if (child.childCount == 0) {
                continue;
            }
            firstChild = child.GetChild(0);
            currentItem = firstChild.GetComponent<Item>();

            // not the item
            if (currentItem.Name != itemName) {
                continue;
            }

            foundItems.Add(currentItem);
            totalQuantity += currentItem.Quantity;
        }

        // player doesn't have quantity of item
        if (totalQuantity < quantity) {
            return false;
        }

        // remove items from smallest stacks until removal quantity fulfilled
        foundItems = foundItems.OrderBy(item => item.Quantity).ToList();
        residual = quantity;
        while (residual > 0) {
            if (foundItems[0].Quantity > residual) {
                foundItems[0].Quantity -= residual;
                return true;
            }
            if(foundItems[0].Quantity <= residual) {
                residual -= foundItems[0].Quantity;
                foundItems[0].gameObject.GetComponentInParent<ItemSlot>().SlotItem = null;
                Destroy(foundItems[0].gameObject);
                foundItems.RemoveAt(0);
            }
        }

        return true;
    }

    // Creates a new object in an empty inventory slot(s)
    // Returns false if there's not enough slots for the quantity
    // Returns true on success
    private bool AddItemAsNewObject(string itemName, int quantity) {
        GameObject newItemObject;
        Item newItem;
        int maxQuantity = Resources.Load<GameObject>("Items/" + itemName).GetComponent<Item>().Capacity;

        List<Transform> emptySlots = new List<Transform>();
        int slotsRequired = (quantity - 1) / maxQuantity + 1; //round up int trick, see https://www.cs.nott.ac.uk/~psarb2/G51MPC/slides/NumberLogic.pdf
        
        // check if player inventory has enough empty slots
        foreach (Transform child in _inventoryContainer.transform) {
            if (child.childCount == 0) {
                emptySlots.Add(child);
                if (emptySlots.Count == slotsRequired) {
                    break;
                }
            }
        }

        // return false if inventory doesn't have enough empty slots
        if (emptySlots.Count < slotsRequired) {
            return false;
        }
        
        // fill empty slots with quantity of item, respecting item max capacity
        int residual = quantity;
        foreach (Transform slot in emptySlots) {
            newItemObject = Instantiate(Resources.Load<GameObject>("Items/" + itemName), slot.position + new Vector3(0, 0, 0), Quaternion.identity, slot);
            slot.GetComponent<ItemSlot>().SlotItem = newItemObject;
            newItem = newItemObject.GetComponent<Item>();

            if (residual <= newItem.Capacity) {
                newItem.Quantity = residual;
                break;
            }
            if (residual > newItem.Capacity) {
                newItem.Quantity = newItem.Capacity;
                residual -= newItem.Capacity;  
            }
        }

        return true;
    }   
}
