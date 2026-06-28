using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Sprite _emptySlotSprite;
    [SerializeField] private Sprite _filledSlotSprite;
    [SerializeField] private int _slotIndex;
    [SerializeField] private Inventory _playerInventory;
    PixelTextRenderer _quantityText;
    Image _itemSprite, _slotSprite;

    private void OnEnable()
    {
        _quantityText = transform.GetChild(1).GetComponent<PixelTextRenderer>();
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
        var _slotItem = inventory.SlotAssignments.ContainsKey(slotIndex) ? inventory.SlotAssignments[slotIndex] : null;

        _slotSprite.sprite = _slotItem != null ? _filledSlotSprite : _emptySlotSprite;
        _itemSprite.enabled = _slotItem != null;

        SetQuantityText(_slotItem?.Quantity ?? 0);

        if (_slotItem != null) {
            _itemSprite.sprite = _slotItem.Item.ItemSprite;
            StartCoroutine(SetItemSpriteToNativeSize());
        }
    }

    private IEnumerator SetItemSpriteToNativeSize() {
        // I think this delay is required so that the items are scaled after the canvas.
        yield return null;
        _itemSprite.SetNativeSize(); 
    }


    private void SetQuantityText(int quantity)
    {
        _quantityText.Text = quantity > 1 ? quantity.ToString() : "";
    }
}

