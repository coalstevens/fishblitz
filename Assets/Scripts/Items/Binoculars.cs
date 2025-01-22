using UnityEngine;

[CreateAssetMenu(fileName = "NewBinoculars", menuName = "Items/Binoculars")]
public class Binoculars : Inventory.ItemType, PlayerInteractionManager.ITool
{
    [SerializeField] private int _energyCost = 1;
    public int EnergyCost => _energyCost;

    public bool UseToolOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        return false;
    }

    public bool UseToolOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation)
    {
        return false;
    }

    public bool UseToolWithoutTarget()
    {
        Debug.Log("Binoculars swung");
        if (PlayerMovementController.Instance.PlayerState.Value == PlayerMovementController.PlayerStates.Birding) {
            Debug.Log("Binoculars returned");
            return false;
        }

        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Birding;
        BirdingGame.Instance.Play();
        return true;
    }

    public void PlayToolHitSound()
    {
        // no sound
    }
}
