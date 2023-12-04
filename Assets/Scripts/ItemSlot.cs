using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReactiveUnity;

public class ItemSlot : MonoBehaviour
{
    private Reactive<GameObject> _slotItem = new Reactive<GameObject>(null);
    private Image _imageRenderer;
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private Sprite fullSlotSprite;

    public GameObject SlotItem {
        get {
            return _slotItem.Value;
        }
        set {
            _slotItem.Value = value;
        }
    } 

    void Start()
    {
        _imageRenderer = GetComponent<Image>();

        if (transform.childCount == 0) {
            _slotItem.Value = null;
        }
        else {
            _slotItem.Value = transform.GetChild(0).gameObject;
        }
        
        UpdateUI();
        _slotItem.OnChange((prev, curr) => UpdateUI());
    }
    
    void UpdateUI() {
        if (_slotItem.Value != null) {
            _imageRenderer.sprite = fullSlotSprite;
        }
        else {
            _imageRenderer.sprite = emptySlotSprite;
        }
    }
}
