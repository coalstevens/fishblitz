using UnityEngine;

public class MountedRod : Inventory.Item
{
    private Inventory _inventory;

    private void Awake()
    {
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
    }
    
    // TODO
    private void PlaceRod(Vector3 cursorLocation)
    {
        // Instantiate(((RodPlacement)interactableTile).RodToPlace, cursorLocation + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
        // _inventory.TryRemoveItem("MountedRod", 1);
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