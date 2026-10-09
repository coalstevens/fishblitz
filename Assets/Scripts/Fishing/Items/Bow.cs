using UnityEngine;

[CreateAssetMenu(fileName = "NewBow", menuName = "Items/Bow")]
public class Bow : RangedWeaponItem, UseItemInput.IUsableWithoutTarget, PlayerEnergyManager.IEnergyDepleting
{
    [Header("Additional")]
    [SerializeField] private int _energyCost;
    [SerializeField] private bool _allowMovementWhileCharging = false;
    [SerializeField] private float _chargeTimeSecs = 1.5f;
    [SerializeField] private float _minSpeedMultiplier = 0.3f;
    [SerializeField] private float _minChargeNormalized = 0.2f;
    [SerializeField] private Vector2 _critShotCharge = new Vector2(0.9f, 0.95f);
    public int EnergyCost => _energyCost;
    public bool AllowMovementWhileCharging => _allowMovementWhileCharging;
    public float ChargeTimeSecs => _chargeTimeSecs;
    public float MinSpeedMultiplier => _minSpeedMultiplier;
    public float MinChargeNormalized => _minChargeNormalized;
    public Vector2 CritShotCharge => _critShotCharge;

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
