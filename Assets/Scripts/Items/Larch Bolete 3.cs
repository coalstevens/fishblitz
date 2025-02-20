using UnityEngine;

[CreateAssetMenu(fileName = "LarchBolete3", menuName = "Items/LarchBolete3")]
public class LarchBolete3 : Inventory.ItemType, Diet.IFood, PlayerInteractionManager.ITool
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

    public void PlayToolHitSound()
    {
        // NOM NOM NOM
    }

    public bool UseToolOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation)
    {
        return false;
    }

    public bool UseToolOnWorldObject(PlayerInteractionManager.IInteractable interactableWorldObject, Vector3Int cursorLocation)
    {
        return false;
    }

    public bool UseToolWithoutTarget()
    {
        if (_inventory.TryRemoveActiveItem(1))
        {
            Diet.EatFood(_playerData, this);
            return true;
        }
        return false;
    }
}
