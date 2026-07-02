using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Sprite _emptySlotSprite;
    [SerializeField] private Sprite _filledSlotSprite;
    [SerializeField] private int _slotIndex;
    [SerializeField] private Inventory _playerInventory;

    [Header("References")]
    [SerializeField] private Image _itemSprite;
    [SerializeField] private PixelCanvasTextRenderer _quantityText;
    [SerializeField] private RectTransform _labelFill;
    [SerializeField] private PixelCanvasTextRenderer _itemLabel;
    [SerializeField] private Image _activeHighlight;

    [Header("Label Settings")]
    [SerializeField] private float _sidePadding = 6f;
    [SerializeField] private float _fadeAfterDurationSecs = 1f;
    [SerializeField] private float _fadeDurationSecs = 0.5f;

    private Image _slotSprite;
    private CanvasGroup _labelCanvasGroup;
    private Coroutine _labelFadeRoutine;
    private Action _unsubscribe;

    private static int _draggedFromSlotIndex = -1;
    private static bool _isDragging = false;
    [SerializeField] private Image _dragGhost;

    private void Awake()
    {
        _slotSprite = GetComponent<Image>();
        _labelCanvasGroup = _labelFill.GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        _playerInventory.SlotUpdated += OnSlotUpdated;
        _unsubscribe = _playerInventory.ActiveItemSlot.OnChange(OnActiveSlotChanged);
        OnSlotUpdated(_playerInventory, _slotIndex);
        OnActiveSlotChanged(_playerInventory.ActiveItemSlot.Value);
    }

    private void OnDisable()
    {
        _playerInventory.SlotUpdated -= OnSlotUpdated;
        _unsubscribe?.Invoke();
        if (_labelFadeRoutine != null)
        {
            StopCoroutine(_labelFadeRoutine);
            _labelFadeRoutine = null;
        }
    }

    private void OnSlotUpdated(Inventory inventory, int slotIndex)
    {
        if (_slotIndex != slotIndex) return;

        var slotItem = inventory.SlotAssignments.TryGetValue(slotIndex, out var stack) ? stack : null;
        bool hasItem = slotItem != null;

        _slotSprite.sprite = hasItem ? _filledSlotSprite : _emptySlotSprite;
        _itemSprite.enabled = hasItem;
        _quantityText.Text = hasItem && slotItem.Quantity > 1 ? slotItem.Quantity.ToString() : "";

        if (hasItem)
        {
            _itemSprite.sprite = slotItem.Item.ItemSprite;
            StartCoroutine(DelayedNativeSize());
        }

        if (_slotIndex == _playerInventory.ActiveItemSlot.Value)
            UpdateLabelForSlot(hasItem ? slotItem : null);
    }

    private void OnActiveSlotChanged(int newSlot)
    {
        bool isActive = _slotIndex == newSlot;
        _activeHighlight.enabled = isActive;

        if (isActive)
        {
            var slotItem = _playerInventory.SlotAssignments.TryGetValue(_slotIndex, out var stack) ? stack : null;
            UpdateLabelForSlot(slotItem);
        }
        else
        {
            HideLabelInstantly();
        }
    }

    private void UpdateLabelForSlot(Inventory.ItemStack slotItem)
    {
        if (slotItem != null && !string.IsNullOrEmpty(slotItem.Item.ItemLabel))
        {
            SetLabelText(slotItem.Item.ItemLabel);
            StartLabelFade();
        }
        else
        {
            HideLabelInstantly();
        }
    }

    private void SetLabelText(string text)
    {
        _itemLabel.Text = text.ToLower();
        ResizeLabel();
    }

    private void ResizeLabel()
    {
        float preferredWidth = _itemLabel.TotalWidth;
        _labelFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth + _sidePadding * 2);
    }

    private void StartLabelFade()
    {
        if (_labelFadeRoutine != null)
            StopCoroutine(_labelFadeRoutine);
        _labelFadeRoutine = StartCoroutine(LabelFadeRoutine());
    }

    private void HideLabelInstantly()
    {
        if (_labelFadeRoutine != null)
        {
            StopCoroutine(_labelFadeRoutine);
            _labelFadeRoutine = null;
        }
        _labelCanvasGroup.alpha = 0;
    }

    private IEnumerator LabelFadeRoutine()
    {
        _labelCanvasGroup.alpha = 1;
        yield return new WaitForSeconds(_fadeAfterDurationSecs);
        float time = 0;
        while (time < _fadeDurationSecs)
        {
            time += Time.deltaTime;
            _labelCanvasGroup.alpha = Mathf.Lerp(1, 0, time / _fadeDurationSecs);
            yield return null;
        }
        _labelCanvasGroup.alpha = 0;
    }

    private IEnumerator DelayedNativeSize()
    {
        yield return null;
        _itemSprite.SetNativeSize();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _playerInventory.ActiveItemSlot.Value = _slotIndex;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_playerInventory.SlotAssignments.ContainsKey(_slotIndex)) return;

        _isDragging = true;
        _draggedFromSlotIndex = _slotIndex;

        _dragGhost.gameObject.SetActive(true);
        _dragGhost.sprite = _itemSprite.sprite;
        _dragGhost.SetNativeSize();
        var rt = (RectTransform)transform;
        Vector3 localPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out localPoint);
        _dragGhost.rectTransform.position = localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        var rt = (RectTransform)transform;
        Vector3 localPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out localPoint);
        _dragGhost.rectTransform.position = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _dragGhost.gameObject.SetActive(false);
        _isDragging = false;
        _draggedFromSlotIndex = -1;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!_isDragging || _draggedFromSlotIndex < 0 || _draggedFromSlotIndex == _slotIndex) return;
        _playerInventory.SwapSlots(_draggedFromSlotIndex, _slotIndex);
    }
}
