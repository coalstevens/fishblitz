using UnityEngine;
using UnityEngine.UI;

public class DryLog : MonoBehaviour, Inventory.IItem, PlayerInteractionManager.IPlayerCursorUsingItem
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

    bool PlayerInteractionManager.IPlayerCursorUsingItem.UseItemOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is LarchStump _larchStump) {
            _larchStump.LoadLog();
            return true;
        }
        return false;
    }

    bool PlayerInteractionManager.IPlayerCursorUsingItem.UseItemOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation)
    {
        return false; // does nothing
    }
}
