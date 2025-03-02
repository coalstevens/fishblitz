using UnityEngine;

[CreateAssetMenu(fileName = "NewDryLog", menuName = "Items/DryLog")]
public class DryLog : Inventory.ItemType, PlayerInteractionManager.IUsableOnWorldObject
{
    public bool UseOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is LarchStump _larchStump) {
            _larchStump.LoadLog();
            return true;
        }
        return false;
    }
}
