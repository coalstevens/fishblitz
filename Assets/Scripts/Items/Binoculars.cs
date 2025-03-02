using UnityEngine;

[CreateAssetMenu(fileName = "NewBinoculars", menuName = "Items/Binoculars")]
public class Binoculars : Inventory.ItemType, PlayerInteractionManager.IUsableWithoutTarget, PlayerInteractionManager.IEnergyDepleting
{
    [SerializeField] private int _energyCost = 1;
    public int EnergyCost => _energyCost;

    public bool UseWithoutTarget()
    {
        if (PlayerMovementController.Instance.PlayerState.Value == PlayerMovementController.PlayerStates.Birding)
            return false;

        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Birding;
        BirdingGame.Instance.Play();
        return true;
    }
}
