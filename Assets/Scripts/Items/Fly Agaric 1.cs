using UnityEngine;

[CreateAssetMenu(fileName = "FlyAgaric1", menuName = "Items/FlyAgaric1")]
public class FlyAgaric1 : Inventory.ItemType, Diet.IFood, UseItemInput.IUsableWithoutTarget
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private int _protein = 10;
    [SerializeField] private int _carbs = 0;
    [SerializeField] private int _nutrients = 0;
    public int Protein => _protein;
    public int Carbs => _carbs;
    public int Nutrients => _nutrients;

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
