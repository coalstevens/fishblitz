using UnityEngine;

public class BarrelBinos : MonoBehaviour, PlayerInteractionManager.IInteractable
{
    [SerializeField] private Inventory _playerInventory;
    [SerializeField] private Inventory.ItemType _binoculars;
    private Barrel _barrel;

    public bool CursorInteract(Vector3 cursorLocation)
    {
        if (_playerInventory.TryAddItem(_binoculars, 1))
        {
            PlayerDialogue.Instance.PostMessage("Looks a little rusty");
            _barrel.RemoveBinoculars();
        }
        else
        {
            PlayerDialogue.Instance.PostMessage("I'm carrying too much already");
        }
        return true;
    }

    void Start()
    {
        _barrel = transform.parent.GetComponent<Barrel>();
    }
}
