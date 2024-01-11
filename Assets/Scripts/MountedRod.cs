using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MountedRod : MonoBehaviour, IPlayerCursorUsingItem, IInventoryItem
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

    public void CursorAction(TileData tileData, Vector3 cursorLocation) {
        PlaceRod(tileData, cursorLocation);
    }

    private void Start()
    {
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
    }
    
    private void PlaceRod(TileData tileData, Vector3 cursorLocation)
    {
        if (tileData.gameObject == null) 
        {
            return;
        }
        bool _canBePlaced = tileData.gameObject.GetComponent<RodPlacement>() != null;

        if (_canBePlaced)
        {
            Vector3 tileLocation = tileData.transform.GetPosition();
            Instantiate(tileData.gameObject.GetComponent<RodPlacement>().RodToPlace, cursorLocation + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
            _inventory.RemoveItem("MountedRod", 1);
        }
    }
}