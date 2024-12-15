using UnityEngine;
using UnityEngine.UI;

public class MountedRod : MonoBehaviour, PlayerInteractionManager.IPlayerCursorUsingItem, Inventory.IItem
{
    private Inventory _inventory;
    private const string ITEM_NAME = "MountedRod";
    private const int STACK_CAPACITY = 10;
    private int _quantity = 0;

    public Sprite ItemSprite { 
        get {
            return GetComponent<Image>().sprite;
        }
        set {
            GetComponent<Image>().sprite = value;
        }
    }
    public string ItemName { get {return ITEM_NAME;}}
    public int Quantity {
        get {
            return _quantity;
        }
        set {
            _quantity = value;
        }
    } 
    public int StackCapacity {get {return STACK_CAPACITY;}}

    private void Start()
    {
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
    }
    
    // TODO
    private void PlaceRod(Vector3 cursorLocation)
    {
        // Instantiate(((RodPlacement)interactableTile).RodToPlace, cursorLocation + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
        // _inventory.TryRemoveItem("MountedRod", 1);
    }

    bool PlayerInteractionManager.IPlayerCursorUsingItem.UseItemOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        return false; // do nothing
    }

    public bool UseItemOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation)
    {
        if (tilemapLayerName == "Water") {
            PlaceRod(cursorLocation);
            return true;
        }
        return false;
    }
}