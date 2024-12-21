using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Sprite _emptySlotSprite;
    [SerializeField] private Sprite _filledSlotSprite;
    [SerializeField] private int _slotIndex;
    [SerializeField] private Inventory _playerInventory;
    TextMeshProUGUI _quantityText;
    Image _itemSprite, _slotSprite;

    private void OnEnable()
    {
        _quantityText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        _itemSprite = transform.GetChild(0).GetComponent<Image>();
        _slotSprite = GetComponent<Image>();

        _playerInventory.SlotUpdated += OnSlotUpdated;
        OnSlotUpdated(_playerInventory, _slotIndex);
    }

    private void OnDisable()
    {
        _playerInventory.SlotUpdated -= OnSlotUpdated;
    }

    private void OnSlotUpdated(Inventory inventory, int slotIndex)
    {
        if (_slotIndex != slotIndex) return;
        if (inventory.SlotItems == null)
            Debug.Log("Slotitems is null");
        var _slotItem = inventory.SlotItems.ContainsKey(slotIndex) ? inventory.SlotItems[slotIndex] : null;

        _slotSprite.sprite = _slotItem != null ? _filledSlotSprite : _emptySlotSprite;
        _itemSprite.enabled = _slotItem != null;

        SetQuantityText(_slotItem?.Quantity ?? 0);

        if (_slotItem != null) _itemSprite.sprite = _slotItem.ItemType.ItemSprite;
    }

    private void SetQuantityText(int quantity)
    {
        _quantityText.text = quantity > 1 ? quantity.ToString() : "";
    }
}

