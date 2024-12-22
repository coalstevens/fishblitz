using UnityEngine;

[CreateAssetMenu(fileName = "NewDryLog", menuName = "Items/DryLog")]
public class DryLog : Inventory.ItemType, PlayerInteractionManager.IPlayerCursorUsingItem
{
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
