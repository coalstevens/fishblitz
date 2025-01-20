using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "NewRod", menuName = "Items/Rod")]
public class Rod : Inventory.ItemType, PlayerInteractionManager.ITool
{
    [SerializeField] private int _energyCost = 2;
    public int EnergyCost => _energyCost;

    bool PlayerInteractionManager.ITool.UseToolOnInteractableTileMap(string tilemapLayerName, UnityEngine.Vector3Int cursorLocation)
    {
        // if fishing stop fishing
        if (PlayerMovementController.Instance.PlayerState.Value == PlayerMovementController.PlayerStates.Fishing) {
            PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Idle;
            FishingGame.Instance.ReelInLine();
            return true;
        }

        // if cursor is on water, start fishing
        if (tilemapLayerName == "Water") {
            PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Fishing;
            FishingGame.Instance.CastForFish();
            return true;
        }
        
        return false;
    }

    public bool UseToolOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        Debug.Log("Used rod on world object");
        return false; // does nothing
    }

    public bool UseToolWithoutTarget()
    {
        Debug.Log("Swung fishing rod");
        return false; // does nothing. Make a casting animation?
    }

    public void PlayToolHitSound()
    {
        // has no sound
    }
}
