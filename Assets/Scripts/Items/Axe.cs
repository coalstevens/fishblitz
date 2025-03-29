using UnityEngine;

[CreateAssetMenu(fileName = "NewAxe", menuName = "Items/Axe")]
public class Axe : Inventory.ItemType, UseItemInput.IUsableOnWorldObject, UseItemInput.IUsableWithoutTarget, PlayerEnergyManager.IEnergyDepleting, UseItemInput.IUsableWithSound
{
    [SerializeField] private int _energyCost = 2;
    public interface IUseableWithAxe : UseItemInput.IUsableTarget
    {
        void OnUseAxe();
    }

    [SerializeField] protected AudioClip _chopSFX;
    [SerializeField] protected float _chopVolume = 1f;

    public int EnergyCost => _energyCost;

    public bool UseWithoutTarget()
    {
        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Axing;
        return false;
    }

    public void PlayHitSound()
    {
        AudioManager.Instance.PlaySFX(_chopSFX, 1f);
    }

    public bool UseOnWorldObject(UseItemInput.IUsableTarget interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is IUseableWithAxe _worldObject)
        {
            PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Axing;
            _worldObject.OnUseAxe();
            return true;
        }
        return false;
    }
}
