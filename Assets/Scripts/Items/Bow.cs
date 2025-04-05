using UnityEngine;

[CreateAssetMenu(fileName = "NewBow", menuName = "Items/Bow")]
public class Bow : RangedWeaponItem, UseItemInput.IUsableWithoutTarget, PlayerEnergyManager.IEnergyDepleting
{
    [Header("Additional")]
    [SerializeField] private int _energyCost;
    public int EnergyCost => _energyCost;

    public bool UseWithoutTarget(Inventory.ItemInstanceData instanceData)
    {
        return TryFire(instanceData as InstanceData);
    }
}
