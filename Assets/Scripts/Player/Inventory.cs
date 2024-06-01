using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ReactiveUnity;
using TMPro;
using System.IO;
using UnityEngine.UI;

[System.Serializable]
public class ItemData {
    public ItemData(string identifier, int quantity) {
        this.identifier = identifier;
        this.quantity = quantity;
    }
    public string identifier;
    public int quantity;
}

public class Inventory : MonoBehaviour
{
    private string _saveFilePath;
    private GameObject _itemCursorController;
    private GameObject _inventoryContainer;
    private int _totalSlots = 10;
    private Dictionary<int, IInventoryItem> _slotAssignments;
    [SerializeField] private Sprite _emptySlotSprite;
    [SerializeField] private Sprite _filledSlotSprite;
    [SerializeField] private ItemData[] _startingItems;
    public Reactive<int> _gold = new Reactive<int>(0);
    public Reactive<int> ActiveItemSlot = new Reactive<int>(0);

    public int Gold 
    {
        get {
            return _gold.Value;
        }
        set {
            _gold.Value = value;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _itemCursorController = GameObject.FindGameObjectWithTag("ItemCursorController");
        _inventoryContainer = GameObject.FindGameObjectWithTag("InventoryContainer");
        _saveFilePath = Path.Combine(Application.persistentDataPath, "InventoryData.json");
        _slotAssignments = LoadInventory();
        UpdateUIAllSlots();

        foreach (ItemData _item in _startingItems) {
            TryAddItem(_item.identifier, _item.quantity);
        }
    }

    /// <summary>
    /// Returns the IInventoryItem of the active slot if slot is not empty.
    /// </summary>
    public IInventoryItem GetActiveItem() {
        if (_slotAssignments.ContainsKey(ActiveItemSlot.Value)) {
            return _slotAssignments[ActiveItemSlot.Value];
        }
        return null;
    }

    public bool TryGetActiveItem(out IInventoryItem _activeItem) {
        if (_slotAssignments.ContainsKey(ActiveItemSlot.Value)) {
            _activeItem = _slotAssignments[ActiveItemSlot.Value];
            return true;
        }
        _activeItem = null;
        return false;
    }
    
    /// <summary>
    /// Adds quantity to existing stacks and creates more stacks if necessary
    /// </summary>
    /// <returns>False if inventory space isn't sufficient, with no change to inventory.</returns>
    public bool TryAddItem(string itemName, int quantity) {
        if (quantity == 0) {
            return true;
        }

        if (!HasEnoughInventorySpace(itemName, quantity)) {
            return false;
        }

        int _residual = quantity;
        foreach (KeyValuePair<int, IInventoryItem> _entry in _slotAssignments) {
            IInventoryItem _currentItem = _entry.Value;
            // not the item
            if (_currentItem.ItemName != itemName) {
                continue;
            }
            // current item has enough capacity to add residual
            if (_residual + _currentItem.Quantity <= _currentItem.StackCapacity) {
                _currentItem.Quantity += _residual;
                UpdateUIAtSlot(_entry.Key);
                SaveInventory();
                return true;
            }
            // increases item quantity to max and calculates residual
            if (_residual + _currentItem.Quantity > _currentItem.StackCapacity) {
                _currentItem.Quantity = _currentItem.StackCapacity;
                _residual = _residual + _currentItem.Quantity - _currentItem.StackCapacity;  
                UpdateUIAtSlot(_entry.Key);
            }
        }

        // All existing stacks of itemName are full
        // create new stack(s)
        AddItemIntoEmptySlots(itemName, _residual);
        SaveInventory();
        return true;
    }

    /// <summary>
    /// Removes a quantity of an item from inventory, starting from smallest stacks
    /// </summary>
    /// <returns>False if inventory doesn't have quantity of item, no change to inventory</returns>
    public bool TryRemoveItem(string itemName, int quantity) {
        List<int> _slotsWithTheItem = new();
        int _playerQuantityOfTheItem = 0;
        int _residual;

        foreach (KeyValuePair<int, IInventoryItem> _entry in _slotAssignments) {
            if (_entry.Value.ItemName == itemName) {
                _slotsWithTheItem.Add(_entry.Key);
                _playerQuantityOfTheItem += _entry.Value.Quantity;
            }
        }
        
        if (_playerQuantityOfTheItem < quantity) {
            return false;
        }

        // remove items from smallest stacks until removal quantity fulfilled
        _slotsWithTheItem = _slotsWithTheItem.OrderBy(slotNum => _slotAssignments[slotNum].Quantity).ToList();
        _residual = quantity;
        foreach (int slot in _slotsWithTheItem) {

            if (_slotAssignments[slot].Quantity > _residual) {
                _slotAssignments[slot].Quantity -= _residual;
                UpdateUIAtSlot(slot); 
                break;
            }
            
            _residual -= _slotAssignments[slot].Quantity;
            
            // Find and destroy the item gameobject in the slot
            foreach (Transform child in _inventoryContainer.transform.GetChild(slot)) {
                if (child.GetComponent<IInventoryItem>() != null) {
                    Destroy(child.gameObject);
                    break;
                }
            }
            _slotAssignments.Remove(slot);
            UpdateUIAtSlot(slot);
        }
        SaveInventory();
        return true;
    }

    // Creates a new object in an empty inventory slot(s)
    private void AddItemIntoEmptySlots(string itemName, int quantity) {
        int _itemStackCapacity = Resources.Load<GameObject>("Items/" + itemName).GetComponent<IInventoryItem>().StackCapacity;
        List<Transform> _emptySlots = new List<Transform>();
        
        // fill empty slots with quantity of item, respecting item stack capacity
        int _residual = quantity;

        for (int i = 0; i < _totalSlots; i++) {
            // if slot isn't empty skip
            if (_slotAssignments.ContainsKey(i)){
                continue;
            }

            // Instantiate new itemName game object as child of slot
            Transform _slot = _inventoryContainer.transform.GetChild(i).transform;
            GameObject _newItemObject = Instantiate(Resources.Load<GameObject>("Items/" + itemName), _slot.position + new Vector3(0, 0, 0), Quaternion.identity, _slot);
            _newItemObject.transform.SetAsFirstSibling(); // So quantity text is on top of item sprite
            IInventoryItem _newItem = _newItemObject.GetComponent<IInventoryItem>();

            // Set quantity of newItem and add to _slotAssignments 
            _slotAssignments.Add(i, _newItem);
            if (_residual <= _newItem.StackCapacity) {
                _newItem.Quantity = _residual;
                UpdateUIAtSlot(i); 
                break;
            }
            else {
                _newItem.Quantity = _newItem.StackCapacity;
                _residual -= _newItem.StackCapacity;  
                UpdateUIAtSlot(i); 
            }      
        }
        return;
    }   

    // Returns true if player has enough inventory space to add quantity of itemName
    public bool HasEnoughInventorySpace(string itemName, int quantity) {
        IInventoryItem _currentItem;
        int _availableSpace = 0;

        // Sum remaining stack space on existing slots with itemName
        foreach (KeyValuePair<int, IInventoryItem> _entry in _slotAssignments) {
            _currentItem = _entry.Value;
            if (_currentItem.ItemName != itemName) {
                continue;
            }

            _availableSpace += _currentItem.StackCapacity - _currentItem.Quantity;
            if (_availableSpace >= quantity) {
                return true;
            }
        }

        // Add to sum the space in empty slots
        int _itemStackCapacity = Resources.Load<GameObject>("Items/" + itemName).GetComponent<IInventoryItem>().StackCapacity;
        int emptySlots = _totalSlots - _slotAssignments.Count;
        _availableSpace += emptySlots * _itemStackCapacity;

        if (_availableSpace >= quantity) {
            return true;
        }
        return false;
    }

    // Updates slot sprite and quantity tag
    private void UpdateUIAtSlot(int slotNum)
    {
        Transform _slot = _inventoryContainer.transform.GetChild(slotNum);
        TextMeshProUGUI _slotQuantityText = _slot.GetComponentInChildren<TextMeshProUGUI>();
        Image _slotImageRenderer = _slot.GetComponent<Image>();
        if (!_slotAssignments.ContainsKey(slotNum)) {
            _slotImageRenderer.sprite = _emptySlotSprite;
            _slotQuantityText.text = "";
            return;
        }

        _slotImageRenderer.sprite = _filledSlotSprite;
        int _itemQuantity = _slotAssignments[slotNum].Quantity;
        if (_itemQuantity == 1)
        {
            _slotQuantityText.text = "";
            return;
        }
        _slotQuantityText.text = _itemQuantity.ToString();
    }

    private void UpdateUIAllSlots() {
        for (int i = 0; i < _totalSlots; i++) {
            UpdateUIAtSlot(i);
        }
    }

    private void SaveInventory() {
        string json = JsonUtility.ToJson(_slotAssignments);
        File.WriteAllText(_saveFilePath, json);
    }
    
    private Dictionary<int, IInventoryItem> LoadInventory() {
        if (File.Exists(_saveFilePath))
        {
            string json = File.ReadAllText(_saveFilePath);
            return JsonUtility.FromJson<Dictionary<int, IInventoryItem>>(json);
        }
        else
        {
            return new Dictionary<int, IInventoryItem>();
        }
    }
    public bool IsPlayerHolding(string itemName) {
        IInventoryItem _activeItem = GetActiveItem();
        if (_activeItem == null)
        {
            return false;
        }
        if (_activeItem.ItemName != itemName)
        {
            return false;
        }
        return true;
    }
}