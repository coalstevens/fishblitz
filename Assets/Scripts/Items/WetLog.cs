using UnityEngine;

[CreateAssetMenu(fileName = "NewWetLog", menuName = "Items/WetLog")]
public class WetLog : Inventory.ItemType, PlayerInteractionManager.IUsableOnWorldObject
{
    [SerializeField] Inventory _inventory;
    public bool UseOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is WoodStove || interactableWorldObject is Campfire) {
            PlayerDialogue.Instance.PostMessage("this is too wet to burn");
            return true;
        }
        if (interactableWorldObject is WoodRack _rack) {
            if (_rack.TryAddWetLog())
                _inventory.TryRemoveActiveItem(1); 
            return true;
        }
        return false;
    }
}
