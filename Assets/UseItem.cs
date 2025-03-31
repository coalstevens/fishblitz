using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UseItemInput : MonoBehaviour
{
    public interface IUsableTarget {
    }

    public interface IUsableOnWorldObject
    {
        /// <summary>
        /// Uses item on the world object under player cursor. Returns false if ignored.
        /// </summary>
        public bool UseOnWorldObject(IUsableTarget interactableWorldObject, Vector3Int cursorLocation);
    }

    public interface IUsableOnTileMap
    {
        /// <summary>
        /// Uses tool on the interactive tilemap under player cursor. Returns false if ignored.
        /// </summary>
        /// ///
        public bool UseOnTileMap(string tilemapLayerName, Vector3Int cursorLocation);
    }

    public interface IUsableWithoutTarget
    {
        /// <returns> True if energy is used </returns>
        public bool UseWithoutTarget();
    }

    public interface IUsableWithSound
    {
        public void PlayHitSound();
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
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Birding ||
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.PickingUp )
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
        Inventory.ItemType _activeItem = _inventory.GetActiveItemType();
        if (_activeItem == null)
        {
            _logger.Info("Active item is null");
            return false;
        }

        if (!_playerEnergyManager.IsSufficientEnergyAvailable(_activeItem as PlayerEnergyManager.IEnergyDepleting)) return true;
        if (TryUseItemOnWorldObject(_activeItem, cursorLocation, targetWorldObject)) return true;
        if (TryUseItemOnTileMap(_activeItem, targetTilemapTag, cursorLocation)) return true;
        if (TryUseItemWithoutTarget(_activeItem)) return true;
        return false;
    }

    private bool TryUseItemOnWorldObject(Inventory.ItemType activeItem, Vector3Int cursorLocation, IUsableTarget interactableWorldObject)
    {
        if (interactableWorldObject != null)
        {
            if (activeItem is IUsableOnWorldObject)
            {
                if (((IUsableOnWorldObject)activeItem).UseOnWorldObject(interactableWorldObject, cursorLocation))
                {
                    DepleteUseEnergy(activeItem);
                    PlayHitSound(activeItem);
                    return true;
                }
            }
        }
        return false;
    }

    private bool TryUseItemOnTileMap(Inventory.ItemType activeItem, string tilemapLayerName, Vector3Int cursorLocation)
    {
        if (activeItem is IUsableOnTileMap)
        {
            if (((IUsableOnTileMap)activeItem).UseOnTileMap(tilemapLayerName, cursorLocation))
            {
                DepleteUseEnergy(activeItem);
                PlayHitSound(activeItem);
                return true;
            }
        }
        return false;
    }

    private bool TryUseItemWithoutTarget(Inventory.ItemType activeItem)
    {
        if (activeItem is IUsableWithoutTarget)
        {
            if (((IUsableWithoutTarget)activeItem).UseWithoutTarget())
            {
                DepleteUseEnergy(activeItem);
                return true;
            }
        }
        return false;
    }

    private void PlayHitSound(Inventory.ItemType activeItem)
    {
        if (activeItem is IUsableWithSound)
            ((IUsableWithSound)activeItem).PlayHitSound();
    }

    private void DepleteUseEnergy(Inventory.ItemType activeItem)
    {
        if (activeItem is PlayerEnergyManager.IEnergyDepleting)
            _playerEnergyManager.DepleteEnergy(((PlayerEnergyManager.IEnergyDepleting)activeItem).EnergyCost);
    }
}
