using UnityEngine;

[CreateAssetMenu(fileName = "NewBinoculars", menuName = "Items/Binoculars")]
public class Binoculars : Inventory.ItemType, PlayerInteractionManager.ITool
{
    public bool UseToolOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        return false;
    }

    public bool UseToolOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation)
    {
        return false;
    }

    public void SwingTool()
    {
        Debug.Log("Binoculars swung");
        if (PlayerMovementController.Instance.PlayerState.Value == PlayerMovementController.PlayerStates.Birding) {
            Debug.Log("Binoculars returned");
            return;
        }

        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Birding;
        BirdingGame.Instance.Play();
    }

    public void PlayToolHitSound()
    {
        // no sound
    }
}
