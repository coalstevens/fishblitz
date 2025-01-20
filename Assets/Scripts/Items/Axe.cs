using UnityEngine;

[CreateAssetMenu(fileName = "NewAxe", menuName = "Items/Axe")]
public class Axe : Inventory.ItemType, PlayerInteractionManager.ITool
{
    [SerializeField] private int _energyCost = 2;
    public interface IUseableWithAxe
    {
        void OnUseAxe();
    }
    [SerializeField] protected AudioClip _chopSFX;

    public int EnergyCost => _energyCost;

    bool PlayerInteractionManager.ITool.UseToolOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        if (interactableWorldObject is IUseableWithAxe _worldObject)
        {
            PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Axing;
            _worldObject.OnUseAxe();
            return true;
        }
        return false;
    }

    bool PlayerInteractionManager.ITool.UseToolWithoutTarget()
    {
        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Axing;
        return false;
    }

    bool PlayerInteractionManager.ITool.UseToolOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation)
    {
        return false; // does nothing
    }

    public void PlayToolHitSound()
    {
        AudioManager.Instance.PlaySFX(_chopSFX, 0.4f);
    }
}
