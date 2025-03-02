using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerInteractionManager : MonoBehaviour
{
    public interface IUsableOnWorldObject
    {
        /// <summary>
        /// Uses item on the world object under player cursor. Returns false if ignored.
        /// </summary>
        public bool UseOnWorldObject(IInteractable interactableWorldObject, Vector3Int cursorLocation);
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

    public interface IEnergyDepleting
    {
        public int EnergyCost { get; }
    }

    public interface IInteractable
    {
        /// <summary>
        /// Returns false if the object ignores the command.
        /// </summary>
        public bool CursorInteract(Vector3 cursorLocation);
    }

    [SerializeField] private Cursor _cursorN;
    [SerializeField] private Cursor _cursorE;
    [SerializeField] private Cursor _cursorS;
    [SerializeField] private Cursor _cursorW;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Logger _logger = new();
    private Grid _grid;
    private PlayerMovementController _playerMovementController;
    private PlayerEnergyManager _playerEnergyManager;
    public Cursor _activeCursor;
    private List<Action> _unsubscribeHooks = new();
    private static readonly List<string> INTERACTABLE_TILEMAP_LAYERS = new List<string> { "Water" };

    private void OnEnable()
    {
        _activeCursor = _cursorE;
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
        _playerEnergyManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerEnergyManager>();
        _unsubscribeHooks.Add(_playerMovementController.FacingDirection.OnChange((prev, curr) => SetPlayerCursorToFacingDirection(curr)));
        SceneManager.sceneLoaded += (scene, mode) => _grid = GameObject.FindFirstObjectByType<Grid>();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= (scene, mode) => _grid = GameObject.FindFirstObjectByType<Grid>();
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

    private void SetPlayerCursorToFacingDirection(FacingDirection curr)
    {
        switch (curr)
        {
            case FacingDirection.North:
                _activeCursor = _cursorN;
                return;
            case FacingDirection.East:
                _activeCursor = _cursorE;
                return;
            case FacingDirection.South:
                _activeCursor = _cursorS;
                return;
            case FacingDirection.West:
                _activeCursor = _cursorW;
                return;
        }
    }

    private void OnUseItem()
    {
        // can't interrupt these
        if (_playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Celebrating ||
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Catching ||
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Axing ||
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Birding)
        {
            return;
        }

        Inventory.ItemType _activeItem = _inventory.GetActiveItemType();
        if (_activeItem == null)
        {
            _logger.Info("Active item is null");
            return;
        }

        if (!IsSufficientEnergyAvailable(_activeItem)) return;

        Vector3Int _cursorLocation = GetActiveCursorLocation();
        IInteractable _interactableWorldObject = FindPlayerCursorInteractableObject(_cursorLocation);
        if (TryUseItemOnWorldObject(_activeItem, _cursorLocation)) return;

        string _interactableTilemapName = FindPlayerCursorInteractableTileMap(_cursorLocation);
        if (TryUseItemOnTileMap(_activeItem, _interactableTilemapName, _cursorLocation)) return;
        if (TryUseItemWithoutTarget(_activeItem)) return;
    }

    private void OnInteract()
    {
        // returns if player is not idle or walking
        if (_playerMovementController.PlayerState.Value != PlayerMovementController.PlayerStates.Idle &&
            _playerMovementController.PlayerState.Value != PlayerMovementController.PlayerStates.Walking)
            return;

        // Check for an interactable object
        Vector3Int _cursorLocation = GetActiveCursorLocation();
        IInteractable _interactableWorldObject = FindPlayerCursorInteractableObject(_cursorLocation);
        if (_interactableWorldObject?.CursorInteract(_cursorLocation) == true)
            return;
    }

    public Vector3Int GetActiveCursorLocation()
    {
        if (_grid == null)
            Debug.LogError("Grid is null, can't find active cursor location");
        return _grid.WorldToCell(_activeCursor.transform.position);
    }

    private bool TryUseItemOnWorldObject(Inventory.ItemType activeItem, Vector3Int cursorLocation)
    {
        IInteractable _interactableWorldObject = FindPlayerCursorInteractableObject(cursorLocation);
        if (_interactableWorldObject != null)
        {
            if (activeItem is IUsableOnWorldObject)
            {
                if (((IUsableOnWorldObject)activeItem).UseOnWorldObject(_interactableWorldObject, cursorLocation))
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

    private bool IsSufficientEnergyAvailable(Inventory.ItemType activeItem)
    {
        if (_playerEnergyManager.IsEnergyAvailable() || activeItem is not IEnergyDepleting)
            return true;
        _logger.Info("Not enough energy remaining.");
        return false;
    }

    private void DepleteUseEnergy(Inventory.ItemType activeItem)
    {
        if (activeItem is IEnergyDepleting)
            _playerEnergyManager.DepleteEnergy(((IEnergyDepleting)activeItem).EnergyCost);
    }

    private IInteractable FindPlayerCursorInteractableObject(Vector3Int cursorLocation)
    {
        List<Collider2D> _results = new List<Collider2D>();
        List<IInteractable> _foundInteractables = new List<IInteractable>();

        // get list of colliders at cursor tile location
        Physics2D.OverlapCollider(_activeCursor.Collider, new ContactFilter2D().NoFilter(), _results);

        // get list of interactables
        foreach (var _result in _results)
        {
            IInteractable _currentObject = _result.GetComponent<IInteractable>();
            if (_currentObject != null)
            {
                _foundInteractables.Add(_currentObject);
            }
        }

        // Only 1 or 0 interactables should be found.
        // Two objects should not occupy the same space
        switch (_foundInteractables.Count)
        {
            case 1:
                return _foundInteractables[0];
            case 0:
                return null;
            default:
                Debug.LogError($"There are {_foundInteractables.Count} interactable objects on this cursor location.");
                return null;
        }
    }

    private string FindPlayerCursorInteractableTileMap(Vector3Int cursorLocation)
    {
        List<string> _foundInteractableLayers = new();
        Tilemap[] _tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);

        // get list of interactable tilemaps at cursorLocation
        foreach (Tilemap _tilemap in _tilemaps)
        {
            if (IsWorldPositionInTilemap(_tilemap, cursorLocation))
            {
                string _layerName = LayerMask.LayerToName(_tilemap.gameObject.layer);
                if (INTERACTABLE_TILEMAP_LAYERS.Contains(_layerName))
                {
                    _foundInteractableLayers.Add(_layerName);
                }
            }
        }

        switch (_foundInteractableLayers.Count)
        {
            case 1:
                return _foundInteractableLayers[0];
            case 0:
                return null;
            default:
                Debug.LogError("There are two interactable tilemaps on this cursor location");
                return null;
        }
    }

    private bool IsWorldPositionInTilemap(Tilemap tilemap, Vector3 worldPosition)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        return tilemap.GetTile(cellPosition) != null;
    }
}
