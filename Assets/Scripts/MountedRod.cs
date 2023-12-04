using System.Collections;
using System.Collections.Generic;
using ReactiveUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class MountedRod : MonoBehaviour, ICursorUsingItem
{
    private InventoryController _inventory;

    public void CursorAction(TileData tileData, Vector3 cursorLocation) {
        PlaceRod(tileData, cursorLocation);
    }

    private void Start()
    {
        _inventory = GameObject.FindWithTag("InventoryContainer").GetComponent<InventoryController>();
    }
    
    private void PlaceRod(TileData tileData, Vector3 cursorLocation)
    {
        if (tileData.gameObject == null) 
        {
            return;
        }
        bool canBePlaced = tileData.gameObject.GetComponent<RodPlacement>() != null;

        if (canBePlaced)
        {
            Vector3 tileLocation = tileData.transform.GetPosition();
            Instantiate(tileData.gameObject.GetComponent<RodPlacement>().RodToPlace, cursorLocation + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
            _inventory.RemoveItem("MountedRod", 1);
        }
    }
}