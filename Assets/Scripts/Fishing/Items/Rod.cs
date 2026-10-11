using UnityEngine;


[CreateAssetMenu(fileName = "NewRod", menuName = "Items/Rod")]
public class Rod : Inventory.Item, UseItemInput.IUsableOnTileMap, PlayerEnergyManager.IEnergyDepleting
{
    [SerializeField] private int _energyCost = 2;
    public int EnergyCost => _energyCost;

    public bool UseOnTileMap(Inventory.ItemInstanceData instanceData, string tilemapLayerName, Vector3Int cursorLocation)
    {
        // if fishing stop fishing
        if (PlayerStateController.Instance.PlayerState.Value == PlayerStates.Fishing) {
            PlayerStateController.Instance.TransitionTo(PlayerStates.Default);
            FishingGame.Instance.ReelInLine();
            return true;
        }

        // if cursor is on water, start fishing
        if (tilemapLayerName == "Water") {
            PlayerStateController.Instance.TransitionTo(PlayerStates.Fishing);
            FishingGame.Instance.CastForFish();
            return true;
        }
        
        return false;
    }
}
