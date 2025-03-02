using UnityEngine;

[CreateAssetMenu(fileName = "NewAxe", menuName = "Items/Axe")]
public class Axe : Inventory.ItemType, PlayerInteractionManager.IUsableOnWorldObject, PlayerInteractionManager.IUsableWithoutTarget, PlayerInteractionManager.IEnergyDepleting
{
    [SerializeField] private int _energyCost = 2;
    public interface IUseableWithAxe
    {
        void OnUseAxe();
    }
    [SerializeField] protected AudioClip _chopSFX;

    public int EnergyCost => _energyCost;

    public void PlayToolHitSound()
    {
        AudioManager.Instance.PlaySFX(_chopSFX, 0.4f);
    }

    public bool UseOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is IUseableWithAxe _worldObject)
        {
            PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Axing;
            _worldObject.OnUseAxe();
            return true;
        }
        return false;
    }

    public bool UseWithoutTarget()
    {
        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Axing;
        return false;
    }
}
