using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReactiveUnity;
using TMPro;
using UnityEngine.Tilemaps;

public class Item : MonoBehaviour
{
    private TextMeshProUGUI _itemQuantityText;
    [SerializeField] private Reactive<int> _quantity = new Reactive<int>(0);
    public string Name;
    public int Capacity;
    public enum ItemType{Misc, Tools, Fish};
    public ItemType itemType;
    
    public int Quantity 
    {
        get {
            return _quantity.Value;
        }
        set {
            _quantity.Value = value;
        }
    }
    private void Start()
    {
        UpdateUiCount();
        HookUpUi();
    }

    private void HookUpUi()
    {
        _quantity.OnChange((prev, curr) => UpdateUiCount());
    }

    private void UpdateUiCount()
    {
        _itemQuantityText = this.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        
        if (_quantity.Value == 1)
        {
            _itemQuantityText.text = "";
            return;
        }

        _itemQuantityText.text = _quantity.Value.ToString();
    }
}

