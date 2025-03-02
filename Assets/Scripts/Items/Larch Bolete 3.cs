using UnityEngine;

[CreateAssetMenu(fileName = "LarchBolete3", menuName = "Items/LarchBolete3")]
public class LarchBolete3 : Inventory.ItemType, Diet.IFood, PlayerInteractionManager.IUsableWithoutTarget
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private int _protein = 10;
    [SerializeField] private int _carbs = 0;
    [SerializeField] private int _nutrients = 0;
    public int Protein => _protein;
    public int Carbs => _carbs;
    public int Nutrients => _nutrients;
    public int EnergyCost => 0;

    public bool UseWithoutTarget()
    {
        if (_inventory.TryRemoveActiveItem(1))
        {
            Diet.EatFood(_playerData, this);
            return true;
        }
        return false;
    }
}
