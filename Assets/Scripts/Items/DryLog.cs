using UnityEngine;

[CreateAssetMenu(fileName = "NewDryLog", menuName = "Items/DryLog")]
public class DryLog : Inventory.ItemType, UseItemInput.IUsableOnWorldObject
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private AudioClip _placeItemSFX;
    [SerializeField] private float _placeItemVolume = 1f;

    public bool UseOnWorldObject(UseItemInput.IUsableTarget interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is LarchStump _larchStump)
        {
            if (_larchStump.TryLoadLog())
            {
                _inventory.TryRemoveActiveItem(1);
                AudioManager.Instance.PlaySFX(_placeItemSFX, _placeItemVolume);
            }
            return true;
        }

        if (interactableWorldObject is WoodRack _rack)
        {
            if (_rack.TryAddDryLog())
            {
                _inventory.TryRemoveActiveItem(1);
                AudioManager.Instance.PlaySFX(_placeItemSFX, _placeItemVolume);
            }
            return true;
        }
        return false;
    }
}
