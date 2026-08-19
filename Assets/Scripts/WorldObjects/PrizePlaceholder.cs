using Newtonsoft.Json;
using UnityEngine;

public class PrizePlaceholder : MonoBehaviour, InteractInput.IInteractable, ISceneSaveable
{
    private const string IDENTIFIER = "PrizePlaceholder";
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] private Inventory.Item _item;
    [SerializeField] private int _itemQuantity;
    [SerializeField] private SoundData _pickUpSound;
    [SerializeField] private AudioSource _audioSource;

    public void SetItem(Inventory.Item item) => _item = item;

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

    private string _persistentID;
    public string PrefabId => IDENTIFIER;
    public string PersistentID { get => _persistentID; set => _persistentID = value; }

    public string CaptureState()
    {
        return JsonConvert.SerializeObject(new object());
    }

    public void RestoreState(string json) { }

    public void ResetState() { }
}
