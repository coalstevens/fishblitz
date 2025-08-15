using UnityEngine;

[CreateAssetMenu(fileName = "NewBinoculars", menuName = "Items/Binoculars")]

public class Binoculars : Inventory.Item, UseItemInput.IUsableWithoutTarget, PlayerEnergyManager.IEnergyDepleting
{
    [SerializeField] private int _energyCost = 1;
    public float BeamRotationSpeedDegreesPerSecond = 75;
    public float BeamAcceleration = 200f;
    public float ChargeTimeSecs = 2f;
    public float CursorTimeSecs = 2f;
    public int EnergyCost => _energyCost;

    public bool UseWithoutTarget(Inventory.ItemInstanceData instanceData)
    {
        if (PlayerMovementController.Instance.PlayerState.Value == PlayerMovementController.PlayerStates.Birding)
        {
            return false;
        }

        PlayerMovementController.Instance.PlayerState.Value = PlayerMovementController.PlayerStates.Birding;
        BirdingGame.Instance.Play(this);
        return true;
    }
}
