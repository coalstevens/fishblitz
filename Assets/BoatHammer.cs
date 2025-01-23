using UnityEngine;

public class BoatHammer : MonoBehaviour, PlayerInteractionManager.IInteractable
{
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] private Inventory.ItemType _hammer;
    BeachedBoat _beachedBoat;

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_playerInventory.TryAddItem(_hammer.ItemName, 1))
        {
            PlayerDialogueController.Instance.PostMessage("Looks a little rusty");
            _beachedBoat.RemoveHammer();
            GameObject.Destroy(this);
        }
        else
        {
            PlayerDialogueController.Instance.PostMessage("I'm carrying too much already");
        }
        return true;
    }

    void Start()
    {
        _beachedBoat = transform.parent.GetComponent<BeachedBoat>();
    }
}
