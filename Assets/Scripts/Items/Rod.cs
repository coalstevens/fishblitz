using UnityEngine;


[CreateAssetMenu(fileName = "NewRod", menuName = "Items/Rod")]
public class Rod : Inventory.ItemType, UseItemInput.IUsableOnTileMap, PlayerEnergyManager.IEnergyDepleting
{
    [SerializeField] private int _energyCost = 2;
    public int EnergyCost => _energyCost;

    public bool UseOnTileMap(string tilemapLayerName, Vector3Int cursorLocation)
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
}
