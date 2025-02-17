using UnityEngine;

public class PickUpOnInteract : MonoBehaviour, PlayerInteractionManager.IInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] private Inventory.ItemType _item;
    [SerializeField] private int _itemQuantity;
    [SerializeField] private AudioClip _pickUpSFX;
    [SerializeField] private float _SFXVolume = 0.5f;

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_playerInventory.TryAddItem(_item.name, _itemQuantity))
        {
            if (_pickUpSFX != null)
                AudioManager.Instance.PlaySFX(_pickUpSFX, _SFXVolume);
            Destroy(gameObject);
        }
        else
        {
            PlayerDialogueController.Instance.PostMessage("I don't have space for this");
        }
        return true;
    }
}
