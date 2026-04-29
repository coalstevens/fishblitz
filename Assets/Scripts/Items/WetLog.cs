using UnityEngine;

[CreateAssetMenu(fileName = "NewWetLog", menuName = "Items/WetLog")]
public class WetLog : Inventory.Item, UseItemInput.IUsableOnWorldObject
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private SoundData _placeItemSound;

    public bool UseOnWorldObject(Inventory.ItemInstanceData instanceData, UseItemInput.IUsableTarget interactableWorldObject, Vector3Int cursorLocation)
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
                AudioManager.PlaySFX(AudioManager.Instance.GetComponent<AudioSource>(), _placeItemSound);
            }
            return true;
        }
        return false;
    }
}
