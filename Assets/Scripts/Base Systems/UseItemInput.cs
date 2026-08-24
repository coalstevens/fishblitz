using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerEnergyManager))]
[RequireComponent(typeof(PlayerCarry))]
[RequireComponent(typeof(PlayerInteraction))]
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

    [SerializeField] private Inventory _inventory;
    [SerializeField] private Logger _logger = new();
    private PlayerInteraction _playerInteraction;
    private PlayerMovement _playerMovementController;
    private PlayerEnergyManager _playerEnergyManager;
    private PlayerCarry _playerCarry;
    private bool _useItemQueued;
    private static readonly List<string> INTERACTABLE_TILEMAP_LAYERS = new List<string> { "Water" };

    private void OnEnable()
    {
        _playerMovementController = GetComponent<PlayerMovement>();
        _playerEnergyManager = GetComponent<PlayerEnergyManager>();
        _playerCarry = GetComponent<PlayerCarry>();
        _playerInteraction = GetComponent<PlayerInteraction>();

        Assert.IsNotNull(_inventory);
    }

    private void Update()
    {
        if (!_useItemQueued) return;
        _useItemQueued = false;

        if (EventSystem.current.IsPointerOverGameObject()) return;

        // can't interrupt these
        if (_playerMovementController.PlayerState.Value == PlayerMovement.PlayerStates.Celebrating ||
            _playerMovementController.PlayerState.Value == PlayerMovement.PlayerStates.Catching ||
            _playerMovementController.PlayerState.Value == PlayerMovement.PlayerStates.Axing ||
            _playerMovementController.PlayerState.Value == PlayerMovement.PlayerStates.PickingUp)
        {
            return;
        }

        IUsableTarget _targetWorldObject = _playerInteraction.FindTarget<IUsableTarget>();
        Vector3Int _cursorLocation = _playerInteraction.GridPosition;
        string _targetTileMapTag = _playerInteraction.FindInteractableTileMapByTags(INTERACTABLE_TILEMAP_LAYERS);

        if (TryUseCarriedObject(_cursorLocation, _targetWorldObject)) return;
        if (TryUseInventoryItem(_cursorLocation, _targetTileMapTag, _targetWorldObject)) return;
    }

    private void OnUseItem()
    {
        _useItemQueued = true;
    }

    private bool TryUseCarriedObject(Vector3Int cursorLocation, IUsableTarget targetWorldObject)
    {
        _logger.Info("Trying to use carried object");
        if (!_playerCarry.IsCarrying.Value)
            return false;

        if (targetWorldObject is Box box)
        {
            if (box.TryAddToBox(_playerCarry.Peek()))
            {
                _playerCarry.Pop();
                return true;
            }
        }
        else if (targetWorldObject is IWeightyObjectContainer _weightyObjectContainer &&
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
