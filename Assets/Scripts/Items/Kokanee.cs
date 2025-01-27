using UnityEngine;

[CreateAssetMenu(fileName = "NewKokanee", menuName = "Items/Kokanee")]
public class Kokanee : Inventory.ItemType, Diet.IFood, PlayerInteractionManager.ITool
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private PlayerData _playerData;
    public int Protein { get; } = 10;
    public int Carbs { get; } = 0;
    public int Nutrients { get; } = 0;
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
