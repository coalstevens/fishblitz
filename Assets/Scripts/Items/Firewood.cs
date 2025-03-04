using UnityEngine;

[CreateAssetMenu(fileName = "NewFirewood", menuName = "Items/Firewood")]
public class Firewood : Inventory.ItemType, PlayerInteractionManager.IUsableOnWorldObject
{
    [SerializeField] private Inventory _playerInventory;
    public bool UseOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is WoodStove _stove)
        {
            if (_stove.AddFirewood())
                _playerInventory.TryRemoveActiveItem(1);
            return true;
        }
        if (interactableWorldObject is Campfire _campfire)
        {
            if (_campfire.AddFirewood())
                _playerInventory.TryRemoveActiveItem(1);
            return true;
        }
        return false;
    }
}
