using UnityEngine;

[CreateAssetMenu(fileName = "NewAxe", menuName = "Items/Axe")]
public class Axe : Inventory.Item, UseItemInput.IUsableOnWorldObject, UseItemInput.IUsableWithoutTarget, PlayerEnergyManager.IEnergyDepleting, UseItemInput.IUsableWithSound
{
    [SerializeField] private int _energyCost = 2;
    public interface IUseableWithAxe : UseItemInput.IUsableTarget
    {
        void OnUseAxe();
    }

    [SerializeField] private SoundData _chopSound;

    public int EnergyCost => _energyCost;

    public bool UseWithoutTarget(Inventory.ItemInstanceData instanceData)
    {
        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Axing;
        return false;
    }

    public void PlayHitSound(Inventory.ItemInstanceData instanceData)
    {
        AudioManager.PlaySFX(AudioManager.Instance.GetComponent<AudioSource>(), _chopSound);
    }

    public bool UseOnWorldObject(Inventory.ItemInstanceData instanceData, UseItemInput.IUsableTarget interactableWorldObject, Vector3Int cursorLocation)
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
