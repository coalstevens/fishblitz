using UnityEngine;

[CreateAssetMenu(fileName = "NewDryLog", menuName = "Items/DryLog")]
public class DryLog : Inventory.Item, UseItemInput.IUsableOnWorldObject
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private SoundData _placeItemSound;

    public bool UseOnWorldObject(Inventory.ItemInstanceData instanceData, UseItemInput.IUsableTarget interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is LarchStump _larchStump)
        {
            if (_larchStump.TryLoadLog())
            {
                _inventory.TryRemoveActiveItem(1);
                AudioManager.PlaySFX(AudioManager.Instance.GetComponent<AudioSource>(), _placeItemSound);
            }
            return true;
        }

        if (interactableWorldObject is WoodRack _rack)
        {
            if (_rack.TryAddDryLog())
            {
                _inventory.TryRemoveActiveItem(1);
                AudioManager.PlaySFX(AudioManager.Instance.GetComponent<AudioSource>(), _placeItemSound);
            }
            return true;
        }
        return false;
    }
}
