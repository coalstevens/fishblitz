using UnityEngine;

// This class is so that the hammer has a unique collider when it's leaning against the boat 
public class BoatHammer : MonoBehaviour, InteractInput.IInteractable
{
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] private Inventory.ItemType _hammer;
    private BeachedBoat _beachedBoat;

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_playerInventory.TryAddItem(_hammer, 1))
        {
            PlayerDialogue.Instance.PostMessage("Looks a little rusty");
            _beachedBoat.RemoveHammer();
        }
        else
        {
            PlayerDialogue.Instance.PostMessage("I'm carrying too much already");
        }
        return true;
    }

    void Start()
    {
        _beachedBoat = transform.parent.GetComponent<BeachedBoat>();
    }
}
