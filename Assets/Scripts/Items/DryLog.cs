using UnityEngine;

[CreateAssetMenu(fileName = "NewDryLog", menuName = "Items/DryLog")]
public class DryLog : Inventory.ItemType, PlayerInteractionManager.IUsableOnWorldObject
{
    [SerializeField] private Inventory _inventory;
    public bool UseOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is LarchStump _larchStump)
        {
            if (_larchStump.TryLoadLog())
                _inventory.TryRemoveActiveItem(1);
            return true;
        }
        if (interactableWorldObject is WoodStove || interactableWorldObject is Campfire)
        {
            PlayerDialogue.Instance.PostMessage("i need to chop this first");
            return true;
        }
        if (interactableWorldObject is WoodRack _rack) 
        {
            if (_rack.TryAddDryLog())
                _inventory.TryRemoveActiveItem(1);
            return true;
        }
        return false;
    }
}
