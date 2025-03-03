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
        public int Quantity;
        public ItemType ItemType;
        public ItemData(ItemType itemType, int quantity)
        {
            Quantity = quantity;
            ItemType = itemType;
        }
    }

    [System.Serializable]
    public class ItemType : ScriptableObject
    {
        public Sprite ItemSprite;
        public string ItemLabel;
        public int StackCapacity;
    }

    [SerializeField] private List<ItemData> _startingItems = new();
    [SerializeField] private Logger _logger = new();
    [SerializeField] private bool _newInventoryOnLoad = true;
    public delegate void SlotUpdateHandler(Inventory inventory, int slotNumber);
    public event SlotUpdateHandler SlotUpdated;
    private string _saveFilePath;
    private int _totalSlots = 10;
    public Dictionary<int, ItemData> SlotItems = new();
    public Reactive<int> ActiveItemSlot = new Reactive<int>(0);

    void OnEnable()
    {
        _saveFilePath = Path.Combine(Application.persistentDataPath, "InventoryData.json");
        SlotItems = LoadInventory();
        foreach (ItemData _item in _startingItems)
            TryAddItem(_item.ItemType, _item.Quantity);
    }

    /// <summary>
    /// Returns the item of the active slot if slot is not empty.
    /// </summary>
    public ItemType GetActiveItemType()
    {
        return SlotItems.ContainsKey(ActiveItemSlot.Value) ? SlotItems[ActiveItemSlot.Value].ItemType : null;
    }
    public ItemData GetActiveItemData()
    {
        return SlotItems.ContainsKey(ActiveItemSlot.Value) ? SlotItems[ActiveItemSlot.Value] : null;
    }

    public bool TryGetActiveItemType(out ItemType _activeItem)
    {
        _activeItem = GetActiveItemType();
        return _activeItem != null;
    }

    /// <summary>
    /// Adds quantity to existing stacks and creates more stacks if necessary
    /// </summary>
    /// <returns>False if inventory space isn't sufficient, with no change to inventory.</returns>
    public bool TryAddItem(ItemType itemType, int quantity)
    {
        if (quantity == 0) return true;
        if (!HasEnoughInventorySpace(itemType.name, quantity)) return false;

        _logger.Info($"Adding {quantity} of {itemType} to inventory");
        int _residual = quantity;
        foreach (var _slot in SlotItems.Where(slot => slot.Value.ItemType.name == itemType.name))
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

        AddItemIntoEmptySlots(itemType.name, _residual);
        SaveInventory();
        return true;
    }

    /// <summary>
    /// Adds item to inventory, or if space is insufficient it is dropped on the ground.
    /// </summary>
    public void AddItemOrDrop(Inventory.ItemType itemType, int quantity, Collider2D spawnCollider)
    {
        if (!TryAddItem(itemType, quantity))
        {
            _logger.Info("Insufficient inventory space. Dropping item instead.");
            SpawnItems.SpawnItemData[] _itemToSpawn = { new SpawnItems.SpawnItemData(itemType, quantity, quantity) };
            SpawnItems.SpawnItemsFromCollider(spawnCollider, _itemToSpawn);
        }
    }

    /// <summary>
    /// Removes a quantity of an item from inventory, starting from smallest stacks
    /// </summary>
    /// <returns>False if inventory doesn't have quantity of item, no change to inventory</returns>
    public bool TryRemoveItem(ItemType itemName, int quantity)
    {
        var _slotsWithTheItem = SlotItems.Where(slot => slot.Value.ItemType.name == itemName.name).OrderBy(slot => slot.Value.Quantity).ToList();
        int _totalQuantity = _slotsWithTheItem.Sum(slot => slot.Value.Quantity);

        _logger.Info($"Attempting to remove {quantity} of {itemName} from inventory. Amount in inventory {_totalQuantity}.");
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

    public bool TryRemoveActiveItem(int quantity)
    {
        if (SlotItems[ActiveItemSlot.Value].Quantity < quantity)
            return false;

        if (SlotItems[ActiveItemSlot.Value].Quantity == quantity)
            SlotItems.Remove(ActiveItemSlot.Value);
        else
            SlotItems[ActiveItemSlot.Value].Quantity -= quantity;   
            
        SlotUpdated?.Invoke(this, ActiveItemSlot.Value);
        return true;
    }

    /// <summary>
    /// Creates a new object in an empty inventory slot(s)
    /// </summary>
    private void AddItemIntoEmptySlots(string itemName, int quantity)
    {
        _logger.Info($"Adding {quantity} of item {itemName} into empty inventory slots.");
        ItemType _itemType = Resources.Load<ItemType>($"Items/{itemName}");
        int _residual = quantity;

        for (int i = 0; i < _totalSlots && _residual > 0; i++)
        {
            if (SlotItems.ContainsKey(i)) continue; // not empty

            var itemData = new ItemData(_itemType, Mathf.Min(_residual, _itemType.StackCapacity));
            SlotItems[i] = itemData;
            _residual -= itemData.Quantity;
            SlotUpdated?.Invoke(this, i);
        }
    }

    // Returns true if player has enough inventory space to add quantity of itemName
    public bool HasEnoughInventorySpace(string itemName, int quantity)
    {
        int availableSpace = SlotItems.Values.Where(x => x.ItemType.name == itemName)
            .Sum(x => x.ItemType.StackCapacity - x.Quantity);

        int emptySlots = _totalSlots - SlotItems.Count;
        int itemStackCapacity = Resources.Load<ItemType>($"Items/{itemName}").StackCapacity;
        availableSpace += emptySlots * itemStackCapacity;

        _logger.Info($"Trying to add {quantity} of {itemName} to Inventory. Available space: {availableSpace}.");
        return availableSpace >= quantity;
    }

    public bool IsPlayerHoldingItem(ItemType itemType)
    {
        ItemData _activeItem = GetActiveItemData();
        _logger.Info($"Checked if player was holding {itemType.name}");
        if (_activeItem == null || _activeItem.ItemType.name != itemType.name)
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
            saveData.items.Add(new InventorySlotData(item.Key, item.Value.ItemType.name, item.Value.Quantity));

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(_saveFilePath, json);
    }

    private Dictionary<int, ItemData> LoadInventory()
    {
        if (File.Exists(_saveFilePath) && !_newInventoryOnLoad)
        {
            string json = File.ReadAllText(_saveFilePath);
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

            Dictionary<int, ItemData> result = new Dictionary<int, ItemData>();
            foreach (var item in saveData.items)
            {
                var itemScript = Resources.Load<ItemType>($"Items/{item.itemName}");
                if (itemScript == null)
                    Debug.LogError($"Cannot find object of type {item.itemName}.");
                else
                    result.Add(item.slotKey, new ItemData(itemScript, item.quantity));
            }
            _logger.Info("Inventory loaded from save.");
            return result;
        }

        _logger.Info("New inventory created.");
        return new Dictionary<int, ItemData>();
    }
}