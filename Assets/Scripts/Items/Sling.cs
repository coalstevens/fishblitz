using UnityEngine;

[CreateAssetMenu(fileName = "NewSling", menuName = "Items/Sling")]
public class Sling : RangedWeaponItem, UseItemInput.IUsableWithoutTarget, PlayerEnergyManager.IEnergyDepleting
{
    [Header("Additional")]
    [SerializeField] private int _energyCost;
    public int EnergyCost => _energyCost;

    public bool UseWithoutTarget(Inventory.ItemInstanceData instanceData)
    {
        return TryFire(instanceData as InstanceData);
    }   
}
