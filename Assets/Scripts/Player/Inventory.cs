using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ReactiveUnity;
using System.IO;

[CreateAssetMenu(fileName = "NewInventory", menuName = "Inventory/Inventory")]
public class Inventory : ScriptableObject
{
    [System.Serializable]
    public class ItemData
    {
        public string ItemName;
        public int Quantity;
        public readonly Item ItemType;
        public ItemData(string itemName, int quantity, Item itemType)
        {
            ItemName = itemName;
            Quantity = quantity;
            ItemType = itemType;
        }
    }
    
    [System.Serializable]
    public class Item : ScriptableObject
    {
        public Sprite ItemSprite;
        public string ItemName;
        public int StackCapacity;
    }

    [SerializeField] private List<ItemData> _startingItems = new();
    public delegate void SlotUpdateHandler(Inventory inventory, int slotNumber);
    public event SlotUpdateHandler SlotUpdated;
    private string _saveFilePath;
    private int _totalSlots = 10;
    public Dictionary<int, ItemData> SlotItems = new();
    public Reactive<int> ActiveItemSlot = new Reactive<int>(0);

    void Awake()
    {
        _saveFilePath = Path.Combine(Application.persistentDataPath, "InventoryData.json");
        SlotItems = LoadInventory();
        foreach (ItemData _item in _startingItems)
            TryAddItem(_item.ItemName, _item.Quantity);
    }

    /// <summary>
    /// Returns the item of the active slot if slot is not empty.
    /// </summary>
    public ItemData GetActiveItem()
    {
        return SlotItems.ContainsKey(ActiveItemSlot.Value) ? SlotItems[ActiveItemSlot.Value] : null;
    }

    public bool TryGetActiveItem(out ItemData _activeItem)
    {
        _activeItem = GetActiveItem();
        return _activeItem != null;
    }

    /// <summary>
    /// Adds quantity to existing stacks and creates more stacks if necessary
    /// </summary>
    /// <returns>False if inventory space isn't sufficient, with no change to inventory.</returns>
    public bool TryAddItem(string itemName, int quantity)
    {
        if (quantity == 0) return true;
        if (!HasEnoughInventorySpace(itemName, quantity)) return false;

        int _residual = quantity;
        foreach (var _slot in SlotItems.Where(slot => slot.Value.ItemName == itemName))
        {
            ItemData _slotItem = _slot.Value;
            int _availableSpace = _slot.Value.ItemType.StackCapacity - _slotItem.Quantity;
            if (_availableSpace >= _residual)
            {
                _slotItem.Quantity += _residual;
                SlotUpdated?.Invoke(this, _slot.Key);
                SaveInventory();
                return true;
            }
            _residual -= _availableSpace;
            _slotItem.Quantity = _slotItem.ItemType.StackCapacity;
            SlotUpdated?.Invoke(this, _slot.Key);
        }

        AddItemIntoEmptySlots(itemName, _residual);
        SaveInventory();
        return true;
    }

    /// <summary>
    /// Adds item to inventory, or if space is insufficient it is dropped on the ground.
    /// </summary>
    public void AddItemOrDrop(string itemName, int quantity, Collider2D spawnCollider, float dropDrag = 1, float dropSpeed = 1)
    {
        if (!TryAddItem(itemName, quantity))
        {
            SpawnItems.ItemSpawnData[] _itemToSpawn = { new SpawnItems.ItemSpawnData(itemName, quantity, quantity) };
            SpawnItems.SpawnItemsFromCollider(spawnCollider, _itemToSpawn, dropSpeed, dropDrag);
        }
    }

    /// <summary>
    /// Removes a quantity of an item from inventory, starting from smallest stacks
    /// </summary>
    /// <returns>False if inventory doesn't have quantity of item, no change to inventory</returns>
    public bool TryRemoveItem(string itemName, int quantity)
    {
        var _slotsWithTheItem = SlotItems.Where(slot => slot.Value.ItemName == itemName).OrderBy(slot => slot.Value.Quantity).ToList();
        int _totalQuantity = _slotsWithTheItem.Sum(slot => slot.Value.Quantity);

        if (_totalQuantity < quantity) return false;

        int _residual = quantity;
        foreach (var _slot in _slotsWithTheItem)
        {
            if (_slot.Value.Quantity > _residual)
            {
                _slot.Value.Quantity -= _residual;
                SlotUpdated?.Invoke(this, _slot.Key);
                break;
            }

            _residual -= _slot.Value.Quantity;
            SlotItems.Remove(_slot.Key);
            SlotUpdated?.Invoke(this, _slot.Key);
        }

        SaveInventory();
        return true;
    }

    /// <summary>
    /// Creates a new object in an empty inventory slot(s)
    /// </summary>
    private void AddItemIntoEmptySlots(string itemName, int quantity)
    {
        Item _itemType = Resources.Load<Item>($"Items/{itemName}");
        int _residual = quantity;

        for (int i = 0; i < _totalSlots && _residual > 0; i++)
        {
            if (SlotItems.ContainsKey(i)) continue; // not empty

            var itemData = new ItemData(itemName, Mathf.Min(_residual, _itemType.StackCapacity), _itemType);
            SlotItems[i] = itemData;
            _residual -= itemData.Quantity;
            SlotUpdated?.Invoke(this, i);
        }
    }

    // Returns true if player has enough inventory space to add quantity of itemName
    public bool HasEnoughInventorySpace(string itemName, int quantity)
    {
        int availableSpace = SlotItems.Values.Where(x => x.ItemName == itemName)
            .Sum(x => x.ItemType.StackCapacity - x.Quantity);

        int emptySlots = _totalSlots - SlotItems.Count;
        int itemStackCapacity = Resources.Load<Item>($"Items/{itemName}").StackCapacity;
        availableSpace += emptySlots * itemStackCapacity;

        return availableSpace >= quantity;
    }

    public bool IsPlayerHoldingItem(string itemName)
    {
        ItemData _activeItem = GetActiveItem();
        if (_activeItem == null || _activeItem.ItemName != itemName)
            return false;
        return true;
    }

    [System.Serializable]
    public class InventorySaveData
    {
        public List<InventorySlotData> items;
    }

    [System.Serializable]
    public class InventorySlotData
    {
        public int slotKey;
        public string itemName;
        public int quantity;

        public InventorySlotData(int key, string name, int qty)
        {
            slotKey = key;
            itemName = name;
            quantity = qty;
        }
    }

    private void SaveInventory()
    {
        InventorySaveData saveData = new InventorySaveData();
        saveData.items = new List<InventorySlotData>();

        foreach (var item in SlotItems)
            saveData.items.Add(new InventorySlotData(item.Key, item.Value.ItemName, item.Value.Quantity));

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(_saveFilePath, json);
    }

    private Dictionary<int, ItemData> LoadInventory()
    {
        if (File.Exists(_saveFilePath))
        {
            string json = File.ReadAllText(_saveFilePath);
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

            Dictionary<int, ItemData> result = new Dictionary<int, ItemData>();
            foreach (var item in saveData.items)
            {
                var itemScript = Resources.Load<Item>($"Items/{item.itemName}");
                if (itemScript == null)
                    Debug.LogError($"Cannot find object of type {item.itemName}.");
                else
                    result.Add(item.slotKey, new ItemData(item.itemName, item.quantity, itemScript));
            }
            return result;
        }

        return new Dictionary<int, ItemData>();
    }
}