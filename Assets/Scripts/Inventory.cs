using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ReactiveUnity;
using TMPro;

public class Inventory : MonoBehaviour
{
    private GameObject _inventoryContainer;
    private GameObject _itemCursorController;

    public Reactive<int> _gold = new Reactive<int>(0);
    public int Gold 
    {
        get {
            return _gold.Value;
        }
        set {
            _gold.Value = value;
        }
    }
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
        Item _currentItem;
        int _residual = quantity;

        if (quantity == 0) {
            return true;
        }

        foreach (Transform _slot in _inventoryContainer.transform) {
            // empty item slot
            if (_slot.childCount == 0) {
                continue;
            }
            
            _currentItem = _slot.GetChild(0).GetComponent<Item>();
            // not the item
            if (_currentItem.Name != itemName) {
                continue;
            }

            // item has enough capacity
            if (_residual + _currentItem.Quantity <= _currentItem.Capacity) {
                _currentItem.Quantity += _residual;
                return true;
            }

            // increases item quantity to max and calculates residual 
            if (_residual + _currentItem.Quantity > _currentItem.Capacity) {
                _currentItem.Quantity = _currentItem.Capacity;
                _residual = _residual + _currentItem.Quantity - _currentItem.Capacity;  
            }
        }

        // loop above only completes if residual > 0
        return AddItemAsNewObject(itemName, _residual);
    }

    // Removes a quantity of an item from inventory
    public bool RemoveItem(string itemName, int quantity) {
        List<Item> _foundItems = new List<Item>();
        Item _currentItem;
        Transform _firstChild;
        int _totalQuantity = 0;
        int _residual;

        foreach (Transform child in _inventoryContainer.transform) {
            // empty item slot
            if (child.childCount == 0) {
                continue;
            }
            _firstChild = child.GetChild(0);
            _currentItem = _firstChild.GetComponent<Item>();

            // not the item
            if (_currentItem.Name != itemName) {
                continue;
            }

            _foundItems.Add(_currentItem);
            _totalQuantity += _currentItem.Quantity;
        }

        // player doesn't have quantity of item
        if (_totalQuantity < quantity) {
            return false;
        }

        // remove items from smallest stacks until removal quantity fulfilled
        _foundItems = _foundItems.OrderBy(item => item.Quantity).ToList();
        _residual = quantity;
        while (_residual > 0) {
            if (_foundItems[0].Quantity > _residual) {
                _foundItems[0].Quantity -= _residual;
                return true;
            }
            if(_foundItems[0].Quantity <= _residual) {
                _residual -= _foundItems[0].Quantity;
                _foundItems[0].gameObject.GetComponentInParent<ItemSlot>().SlotItem = null;
                Destroy(_foundItems[0].gameObject);
                _foundItems.RemoveAt(0);
            }
        }

        return true;
    }

    // Creates a new object in an empty inventory slot(s)
    // Returns false if there's not enough slots for the quantity
    // Returns true on success
    private bool AddItemAsNewObject(string itemName, int quantity) {
        GameObject _newItemObject;
        Item _newItem;
        int _maxQuantity = Resources.Load<GameObject>("Items/" + itemName).GetComponent<Item>().Capacity;

        List<Transform> _emptySlots = new List<Transform>();
        int _slotsRequired = (quantity - 1) / _maxQuantity + 1; //round up int trick, see https://www.cs.nott.ac.uk/~psarb2/G51MPC/slides/NumberLogic.pdf
        
        // check if player inventory has enough empty slots
        foreach (Transform _child in _inventoryContainer.transform) {
            if (_child.childCount == 0) {
                _emptySlots.Add(_child);
                if (_emptySlots.Count == _slotsRequired) {
                    break;
                }
            }
        }

        // return false if inventory doesn't have enough empty slots
        if (_emptySlots.Count < _slotsRequired) {
            return false;
        }
        
        // fill empty slots with quantity of item, respecting item max capacity
        int _residual = quantity;
        foreach (Transform _slot in _emptySlots) {
            _newItemObject = Instantiate(Resources.Load<GameObject>("Items/" + itemName), _slot.position + new Vector3(0, 0, 0), Quaternion.identity, _slot);
            _slot.GetComponent<ItemSlot>().SlotItem = _newItemObject;
            _newItem = _newItemObject.GetComponent<Item>();

            if (_residual <= _newItem.Capacity) {
                _newItem.Quantity = _residual;
                break;
            }
            if (_residual > _newItem.Capacity) {
                _newItem.Quantity = _newItem.Capacity;
                _residual -= _newItem.Capacity;  
            }
        }

        return true;
    }   
}
