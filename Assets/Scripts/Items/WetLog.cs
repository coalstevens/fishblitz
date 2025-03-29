using UnityEngine;

[CreateAssetMenu(fileName = "NewWetLog", menuName = "Items/WetLog")]
public class WetLog : Inventory.ItemType, UseItemInput.IUsableOnWorldObject
{
    [SerializeField] Inventory _inventory;
    [SerializeField] private AudioClip _placeItemSFX;
    [SerializeField] private float _placeItemVolume = 1f;

    public bool UseOnWorldObject(UseItemInput.IUsableTarget interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is WoodStove || interactableWorldObject is Campfire)
        {
            PlayerDialogue.Instance.PostMessage("this is too wet to burn");
            return true;
        }
        if (interactableWorldObject is WoodRack _rack)
        {
            if (_rack.TryAddWetLog())
            {
                _inventory.TryRemoveActiveItem(1);
                AudioManager.Instance.PlaySFX(_placeItemSFX, _placeItemVolume);
            }
            return true;
        }
        return false;
    }
}
