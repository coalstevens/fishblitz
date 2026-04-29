using UnityEngine;

public class PickUpOnInteract : MonoBehaviour, InteractInput.IInteractable
{
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] private Inventory.Item _item;
    [SerializeField] private int _itemQuantity;
    [SerializeField] private SoundData _pickUpSound;
    [SerializeField] private AudioSource _audioSource;

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_playerInventory.TryAddItem(_item, _itemQuantity))
        {
            if (_pickUpSound != null)
                AudioManager.PlaySFX(_audioSource, _pickUpSound);
            Destroy(gameObject);
        }
        else
        {
            PlayerDialogue.Instance.PostMessage("I don't have space for this");
        }
        return true;
    }
}
