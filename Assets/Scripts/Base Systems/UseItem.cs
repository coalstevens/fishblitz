using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UseItemInput : MonoBehaviour
{
    public interface IUsableTarget
    {
    }

    public interface IUsableOnWorldObject
    {
        /// <summary>
        /// Uses item on the world object under player cursor. Returns false if ignored.
        /// </summary>
        public bool UseOnWorldObject(Inventory.ItemInstanceData instanceData, IUsableTarget interactableWorldObject, Vector3Int cursorLocation);
    }

    public interface IUsableOnTileMap
    {
        /// <summary>
        /// Uses tool on the interactive tilemap under player cursor. Returns false if ignored.
        /// </summary>
        /// ///
        public bool UseOnTileMap(Inventory.ItemInstanceData instanceData, string tilemapLayerName, Vector3Int cursorLocation);
    }

    public interface IUsableWithoutTarget
    {
        /// <returns> True if energy is used </returns>
        public bool UseWithoutTarget(Inventory.ItemInstanceData instanceData);
    }

    public interface IUsableWithSound
    {
        public void PlayHitSound(Inventory.ItemInstanceData instanceData);
    }

    [SerializeField] private GridCursor _gridCursor;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Logger _logger = new();
    private PlayerMovementController _playerMovementController;
    private PlayerEnergyManager _playerEnergyManager;
    private PlayerCarry _playerCarry;
    private static readonly List<string> INTERACTABLE_TILEMAP_LAYERS = new List<string> { "Water" };

    private void OnEnable()
    {
        _playerMovementController = GetComponent<PlayerMovementController>();
        _playerEnergyManager = GetComponent<PlayerEnergyManager>();
        _playerCarry = GetComponent<PlayerCarry>();

        Assert.IsNotNull(_playerMovementController);
        Assert.IsNotNull(_playerEnergyManager);
        Assert.IsNotNull(_playerCarry);
        Assert.IsNotNull(_playerData);
        Assert.IsNotNull(_gridCursor);
        Assert.IsNotNull(_inventory);
    }

    private void OnUseItem()
    {
        // can't interrupt these
        if (_playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Celebrating ||
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Catching ||
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Axing ||
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.PickingUp)
        {
            return;
        }

        Vector3Int _cursorLocation = _gridCursor.GridPosition;
        IUsableTarget _targetWorldObject = _gridCursor.FindObjectAtGridCursor<IUsableTarget>();
        string _targetTileMapTag = _gridCursor.FindInteractableTileMapByTags(INTERACTABLE_TILEMAP_LAYERS);

        if (TryUseCarriedObject(_cursorLocation, _targetWorldObject)) return;
        if (TryUseInventoryItem(_cursorLocation, _targetTileMapTag, _targetWorldObject)) return;
    }

    private bool TryUseCarriedObject(Vector3Int cursorLocation, IUsableTarget targetWorldObject)
    {
        _logger.Info("Trying to use carried object");
        if (!_playerData.IsCarrying.Value)
            return false;

        // to use a carried object is to put it down or put it in a container
        if (targetWorldObject is IWeightyObjectContainer _weightyObjectContainer &&
            _weightyObjectContainer.WeightyStack.HasEnoughSpace(_playerCarry.Peek().Type.Weight))
        {
            _weightyObjectContainer.WeightyStack.Push(_playerCarry.Pop());
        }
        else
        {
            _playerCarry.PutDown(cursorLocation);
        }

        return true;
    }

    private bool TryUseInventoryItem(Vector3Int cursorLocation, string targetTilemapTag, IUsableTarget targetWorldObject)
    {
        _logger.Info("Trying to use inventory item");
        Inventory.Item _activeItem = _inventory.GetActiveItem();
        Inventory.ItemInstanceData _activeItemInstanceData = _inventory.GetActiveItemInstanceData();

        if (_activeItem == null)
        {
            _logger.Info("Active item is null");
            return false;
        }

        if (!_playerEnergyManager.IsSufficientEnergyAvailable(_activeItem as PlayerEnergyManager.IEnergyDepleting)) return true;
        if (TryUseItemOnWorldObject(_activeItem, _activeItemInstanceData, cursorLocation, targetWorldObject)) return true;
        if (TryUseItemOnTileMap(_activeItem, _activeItemInstanceData, targetTilemapTag, cursorLocation)) return true;
        if (TryUseItemWithoutTarget(_activeItem, _activeItemInstanceData)) return true;
        return false;
    }

    private bool TryUseItemOnWorldObject(Inventory.Item item, Inventory.ItemInstanceData instanceData, Vector3Int cursorLocation, IUsableTarget interactableWorldObject)
    {
        _logger.Info("Trying to use item on world object");
        if (interactableWorldObject != null)
        {
            if (item is IUsableOnWorldObject)
            {
                if (((IUsableOnWorldObject)item).UseOnWorldObject(instanceData, interactableWorldObject, cursorLocation))
                {
                    DepleteUseEnergy(item, instanceData);
                    PlayHitSound(item, instanceData);
                    return true;
                }
            }
        }
        return false;
    }

    private bool TryUseItemOnTileMap(Inventory.Item item, Inventory.ItemInstanceData instanceData, string tilemapLayerName, Vector3Int cursorLocation)
    {
        _logger.Info("Trying to use item on tilemap");
        if (item is IUsableOnTileMap)
        {
            if (((IUsableOnTileMap)item).UseOnTileMap(instanceData, tilemapLayerName, cursorLocation))
            {
                DepleteUseEnergy(item, instanceData);
                PlayHitSound(item, instanceData);
                return true;
            }
        }
        return false;
    }

    private bool TryUseItemWithoutTarget(Inventory.Item item, Inventory.ItemInstanceData instanceData)
    {
        _logger.Info("Trying to use item without target");
        if (item is IUsableWithoutTarget)
        {
            if (((IUsableWithoutTarget)item).UseWithoutTarget(instanceData))
            {
                DepleteUseEnergy(item, instanceData);
                return true;
            }
        }
        return false;
    }

    private void PlayHitSound(Inventory.Item item, Inventory.ItemInstanceData instanceData)
    {
        if (item is IUsableWithSound)
            ((IUsableWithSound)item).PlayHitSound(instanceData);
    }

    private void DepleteUseEnergy(Inventory.Item activeItem, Inventory.ItemInstanceData instanceData)
    {
        if (activeItem is PlayerEnergyManager.IEnergyDepleting)
            _playerEnergyManager.DepleteEnergy(((PlayerEnergyManager.IEnergyDepleting)activeItem).EnergyCost);
    }
}
