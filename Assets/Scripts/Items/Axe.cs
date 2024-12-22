using UnityEngine;

[CreateAssetMenu(fileName = "NewAxe", menuName = "Items/Axe")]
public class Axe : Inventory.ItemType, PlayerInteractionManager.ITool
{
    public interface IUseableWithAxe
    {
        void OnUseAxe();
    }
    [SerializeField] protected AudioClip _chopSFX;

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

    void PlayerInteractionManager.ITool.SwingTool()
    {
        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Axing;
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
