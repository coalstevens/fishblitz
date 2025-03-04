using UnityEngine;

[CreateAssetMenu(fileName = "NewAxe", menuName = "Items/Axe")]
public class Axe : Inventory.ItemType, PlayerInteractionManager.IUsableOnWorldObject, PlayerInteractionManager.IUsableWithoutTarget, PlayerInteractionManager.IEnergyDepleting, PlayerInteractionManager.IUsableWithSound
{
    [SerializeField] private int _energyCost = 2;
    public interface IUseableWithAxe
    {
        void OnUseAxe();
    }
    [SerializeField] protected AudioClip _chopSFX;
    [SerializeField] protected float _chopVolume = 1f;

    public int EnergyCost => _energyCost;

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

    public void PlayHitSound()
    {
        AudioManager.Instance.PlaySFX(_chopSFX, 1f);
    }
}
