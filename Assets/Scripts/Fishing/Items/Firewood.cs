using UnityEngine;

[CreateAssetMenu(fileName = "NewFirewood", menuName = "Items/Firewood")]
public class Firewood : Inventory.Item, UseItemInput.IUsableOnWorldObject
{
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] private SoundData _placeItemSound;

    public bool UseOnWorldObject(Inventory.ItemInstanceData instanceData, UseItemInput.IUsableTarget interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is WoodStove _stove)
        {
            if (_stove.AddFirewood())
            {
                _playerInventory.TryRemoveActiveItem(1);
                AudioManager.PlaySFX(AudioManager.Instance.GetComponent<AudioSource>(), _placeItemSound);
            }
            return true;
        }
        if (interactableWorldObject is Campfire _campfire)
        {
            if (_campfire.AddFirewood())
            {
                _playerInventory.TryRemoveActiveItem(1);
                AudioManager.PlaySFX(AudioManager.Instance.GetComponent<AudioSource>(), _placeItemSound);
            }
            return true;
        }
        return false;
    }
}
