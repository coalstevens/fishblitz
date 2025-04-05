using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ReactiveUnity;
using System.IO;
using System;

[CreateAssetMenu(fileName = "NewInventory", menuName = "Inventory/Inventory")]
public class Inventory : ScriptableObject
{
    public interface IInstancedItem<out T> where T: ItemInstanceData
    {
        T CreateInstanceData();
    }

    [Serializable]
    public abstract class ItemInstanceData
    {
        public Guid InstancedID = Guid.NewGuid();
    }

    [Serializable]
    public class ItemStack
    {
        public int Quantity;
        public Item Item;
        public ItemInstanceData ItemInstanceData; // optional
        public ItemStack(Item item, int quantity)
        {
            Quantity = quantity;
            Item = item;

            if (item is IInstancedItem<ItemInstanceData> instanced)
                ItemInstanceData = instanced.CreateInstanceData();
        }
    }

    [Serializable]
    public abstract class Item : ScriptableObject
    {
        [Header("Item")]
        public Sprite ItemSprite;
        public string ItemLabel;
        public int StackCapacity;
    }

    [SerializeField] private List<ItemStack> _startingItemStacks = new();
    [SerializeField] private Logger _logger = new();
    [SerializeField] private bool _newInventoryOnLoad = true;
    [SerializeField] private AudioClip _addItemSFX;
    [SerializeField] private float _addItemVolume = 1f;

    public delegate void SlotUpdateHandler(Inventory inventory, int slotNumber);
    public event SlotUpdateHandler SlotUpdated;

    private string _saveFilePath;
    private int _totalSlots = 10;
    public Dictionary<int, ItemStack> SlotAssignments = new();
    public Reactive<int> ActiveItemSlot = new Reactive<int>(0);

    void OnEnable()
    {
        _saveFilePath = Path.Combine(Application.persistentDataPath, "InventoryData.json");
        SlotAssignments = LoadInventory();
        foreach (ItemStack _item in _startingItemStacks)
            TryAddItem(_item.Item, _item.Quantity);
    }

    /// <summary>
    /// Returns the item of the active slot if slot is not empty.
    /// </summary>
    public Item GetActiveItem()
    {
        return SlotAssignments.ContainsKey(ActiveItemSlot.Value) ? SlotAssignments[ActiveItemSlot.Value].Item : null;
    }

    public ItemInstanceData GetActiveItemInstanceData()
    {
        return SlotAssignments.ContainsKey(ActiveItemSlot.Value) ? SlotAssignments[ActiveItemSlot.Value].ItemInstanceData : null;
    }

    public bool TryGetActiveItemType(out Item _activeItem)
    {
        _activeItem = GetActiveItem();
        return _activeItem != null;
    }

    /// <summary>
    /// Adds quantity to existing stacks and creates more stacks if necessary
    /// </summary>
    /// <returns>False if inventory space isn't sufficient, with no change to inventory.</returns>
    public bool TryAddItem(Item itemType, int quantity)
    {
        if (quantity == 0) return true;
        if (!HasEnoughInventorySpace(itemType.name, quantity)) return false;

        _logger.Info($"Adding {quantity} of {itemType} to inventory");
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(_addItemSFX, _addItemVolume);

        int _residual = quantity;
        foreach (var _slot in SlotAssignments.Where(slot => slot.Value.Item.name == itemType.name))
        {
            ItemStack _slotItem = _slot.Value;
            int _availableSpace = _slot.Value.Item.StackCapacity - _slotItem.Quantity;
            if (_availableSpace >= _residual)
            {
                _slotItem.Quantity += _residual;
                SlotUpdated?.Invoke(this, _slot.Key);
                SaveInventory();
                return true;
            }
            _residual -= _availableSpace;
            _slotItem.Quantity = _slotItem.Item.StackCapacity;
            SlotUpdated?.Invoke(this, _slot.Key);
        }

        AddItemIntoEmptySlots(itemType.name, _residual);
        SaveInventory();
        return true;
    }

    /// <summary>
    /// Adds item to inventory, or if space is insufficient it is dropped on the ground.
    /// </summary>
    public void AddItemOrDrop(Inventory.Item itemType, int quantity, Collider2D spawnCollider)
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
    public bool TryRemoveItem(Item itemName, int quantity)
    {
        var _slotsWithTheItem = SlotAssignments.Where(slot => slot.Value.Item.name == itemName.name).OrderBy(slot => slot.Value.Quantity).ToList();
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
            SlotAssignments.Remove(_slot.Key);
            SlotUpdated?.Invoke(this, _slot.Key);
        }

        SaveInventory();
        return true;
    }

    public bool TryRemoveActiveItem(int quantity)
    {
        if (SlotAssignments[ActiveItemSlot.Value].Quantity < quantity)
            return false;

        if (SlotAssignments[ActiveItemSlot.Value].Quantity == quantity)
            SlotAssignments.Remove(ActiveItemSlot.Value);
        else
            SlotAssignments[ActiveItemSlot.Value].Quantity -= quantity;

        SlotUpdated?.Invoke(this, ActiveItemSlot.Value);
        return true;
    }

    /// <summary>
    /// Creates a new object in an empty inventory slot(s)
    /// </summary>
    private void AddItemIntoEmptySlots(string itemName, int quantity)
    {
        _logger.Info($"Adding {quantity} of item {itemName} into empty inventory slots.");
        Item _itemType = Resources.Load<Item>($"Items/{itemName}");
        int _residual = quantity;

        for (int i = 0; i < _totalSlots && _residual > 0; i++)
        {
            if (SlotAssignments.ContainsKey(i)) continue; // not empty

            var itemData = new ItemStack(_itemType, Mathf.Min(_residual, _itemType.StackCapacity));
            SlotAssignments[i] = itemData;
            _residual -= itemData.Quantity;
            SlotUpdated?.Invoke(this, i);
        }
    }

    // Returns true if player has enough inventory space to add quantity of itemName
    public bool HasEnoughInventorySpace(string itemName, int quantity)
    {
        int availableSpace = SlotAssignments.Values.Where(x => x.Item.name == itemName)
            .Sum(x => x.Item.StackCapacity - x.Quantity);

        int emptySlots = _totalSlots - SlotAssignments.Count;
        int itemStackCapacity = Resources.Load<Item>($"Items/{itemName}").StackCapacity;
        availableSpace += emptySlots * itemStackCapacity;

        _logger.Info($"Trying to add {quantity} of {itemName} to Inventory. Available space: {availableSpace}.");
        return availableSpace >= quantity;
    }

    public bool IsPlayerHoldingItem(Item item)
    {
        Item _activeItem = GetActiveItem();
        _logger.Info($"Checked if player was holding {item.name}");
        if (_activeItem == null || _activeItem.name != item.name)
            return false;
        return true;
    }

    [Serializable]
    public class InventorySaveData
    {
        public List<SlotSaveData> items;
    }

    [Serializable]
    public class SlotSaveData
    {
        public int SlotKey;
        public string ItemName;
        public int Quantity;
        public string InstanceJson;

        public SlotSaveData(int key, string name, int qty, string instanceDataJson = null)
        {
            SlotKey = key;
            ItemName = name;
            Quantity = qty;
            InstanceJson = instanceDataJson;
        }
    }

    private void SaveInventory()
    {
        InventorySaveData saveData = new InventorySaveData();
        saveData.items = new List<SlotSaveData>();

        foreach (var item in SlotAssignments)
        {
            string instanceJson = null;

            if (item.Value.ItemInstanceData != null)
                instanceJson = JsonUtility.ToJson(item.Value.ItemInstanceData);

            saveData.items.Add(new SlotSaveData(
                item.Key,
                item.Value.Item.name,
                item.Value.Quantity,
                instanceJson
            ));
        }

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(_saveFilePath, json);
    }

    private Dictionary<int, ItemStack> LoadInventory()
    {
        if (File.Exists(_saveFilePath) && !_newInventoryOnLoad)
        {
            string json = File.ReadAllText(_saveFilePath);
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

            Dictionary<int, ItemStack> result = new Dictionary<int, ItemStack>();
            foreach (var item in saveData.items)
            {
                var itemScript = Resources.Load<Item>($"Items/{item.ItemName}");
                if (itemScript == null)
                {
                    Debug.LogError($"Cannot find object of type {item.ItemName}.");
                    continue;
                }

                var stack = new ItemStack(itemScript, item.Quantity);

                if (!string.IsNullOrEmpty(item.InstanceJson) && itemScript is IInstancedItem<ItemInstanceData> instanced)
                {
                    Type instanceType = instanced.CreateInstanceData().GetType();
                    stack.ItemInstanceData = (ItemInstanceData)JsonUtility.FromJson(item.InstanceJson, instanceType);
                }

                result.Add(item.SlotKey, stack);
            }
            _logger.Info("Inventory loaded from save.");
            return result;
        }

        _logger.Info("New inventory created.");
        return new Dictionary<int, ItemStack>();
    }
}