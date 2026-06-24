using UnityEngine;

[CreateAssetMenu(fileName = "NewBow", menuName = "Items/Bow")]
public class Bow : RangedWeaponItem, UseItemInput.IUsableWithoutTarget, PlayerEnergyManager.IEnergyDepleting
{
    [Header("Additional")]
    [SerializeField] private int _energyCost;
    [SerializeField] private bool _allowMovementWhileCharging = false;
    public int EnergyCost => _energyCost;
    public bool AllowMovementWhileCharging => _allowMovementWhileCharging;

    public bool UseWithoutTarget(Inventory.ItemInstanceData instanceData)
    {
        var weaponData = instanceData as InstanceData;
        if (weaponData == null || weaponData.ProjectileSpawnCenter == null)
            return TryFire(weaponData);

        var controller = weaponData.ProjectileSpawnCenter.GetComponent<BowChargeController>();
        if (controller == null)
            return TryFire(weaponData);

        controller.StartCharge(this, weaponData);
        return false;
    }
}
