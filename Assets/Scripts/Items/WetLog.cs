using UnityEngine;

[CreateAssetMenu(fileName = "NewWetLog", menuName = "Items/WetLog")]
public class WetLog : Inventory.ItemType, PlayerInteractionManager.IUsableOnWorldObject
{
    public bool UseOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is WoodStove || interactableWorldObject is Campfire) {
            PlayerDialogue.Instance.PostMessage("this is too wet to burn");
            return true;
        }
        return false;
    }
}
