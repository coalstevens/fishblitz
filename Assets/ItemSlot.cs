using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReactiveUnity;

public class ItemSlot : MonoBehaviour
{
    private Reactive<bool> _hasItem = new Reactive<bool>(false);
    private GameObject _slotItem;
    private Image _imageRenderer;
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private Sprite fullSlotSprite;

    public GameObject SlotItem {
        get {
            return _slotItem;
        }
        set {
            if (value != null) {
                _slotItem = value;
                _hasItem.Value = true;
            }
            else {
                _slotItem = null;
                _hasItem.Value = false;
            }
        }
    } 

    void Start()
    {
        _imageRenderer = GetComponent<Image>();

        if (transform.childCount == 0) {
            _slotItem = null;
        }
        else {
            _slotItem = transform.GetChild(0).gameObject;
            _hasItem.Value = true;
        }
        
        UpdateUI();
        _hasItem.OnChange((prev, curr) => UpdateUI());
    }
    
    void UpdateUI() {
        if (_hasItem.Value) {
            _imageRenderer.sprite = fullSlotSprite;
        }
        else {
            _imageRenderer.sprite = emptySlotSprite;
        }
    }
}
