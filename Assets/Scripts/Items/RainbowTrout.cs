using UnityEngine;
using UnityEngine.UI;

public class RainbowTrout : MonoBehaviour, Inventory.IItem
{
    private const string ITEM_HAME = "RainbowTrout";
    private int _quantity = 0;
    private const int STACK_CAPACITY = 99;
    public int StackCapacity {get {return STACK_CAPACITY;}}
    public string ItemName { get {return ITEM_HAME;} }

    public Sprite ItemSprite { 
        get => GetComponent<Image>().sprite;
        set => GetComponent<Image>().sprite = value;
    }

    public int Quantity { 
        get => _quantity;
        set => _quantity = value;
    }
}
