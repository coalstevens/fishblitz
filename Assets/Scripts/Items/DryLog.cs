using UnityEngine;
using UnityEngine.UI;

public class DryLog : MonoBehaviour, IInventoryItem, IPlayerCursorUsingItem
{
    private const string ITEM_HAME = "DryLog";
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

    public void UseItemOnWorldObject(IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is Larch _larch) {
            _larch.LoadLog();
        }
    }

    public void UseItemOnTile(IInteractableTile interactableTile, Vector3Int cursorLocation)
    {
        // do nothing
    }
}
