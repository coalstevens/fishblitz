using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerInteractionManager : MonoBehaviour
{
    public interface ITool
    {
        /// <summary>
        /// Uses tool on the world object under player object. Returns false if ignored.
        /// </summary>
        public bool UseToolOnWorldObject(IInteractable interactableWorldObject, Vector3Int cursorLocation);

        /// <summary>
        /// Uses tool on the interactive tilemap under cursor. Returns false if ignored.
        /// </summary>
        public bool UseToolOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation);

        /// <summary>
        /// Uses tool on no object in particular 
        /// </summary>
        /// <returns> True if energy is used </returns>
        public bool UseToolWithoutTarget();

        /// <summary>
        /// Plays a sound when the tool interacts with the target
        /// </summary>
        public void PlayToolHitSound();

        /// <summary>
        /// Returns the energy cost of using the tool
        /// </summary>
        public int EnergyCost { get; }
    }

    public interface IInteractable
    {
        /// <summary>
        /// Returns false if the object ignores the command.
        /// </summary>
        public bool CursorInteract(Vector3 cursorLocation);
    }

    public interface IPlayerCursorUsingItem
    {
        public bool UseItemOnWorldObject(IInteractable interactableWorldObject, Vector3Int cursorLocation);
        public bool UseItemOnInteractableTileMap(string tilemapLayerName, Vector3Int cursorLocation);
    }

    [SerializeField] private Cursor _cursorN;
    [SerializeField] private Cursor _cursorE;
    [SerializeField] private Cursor _cursorS;
    [SerializeField] private Cursor _cursorW;
    [SerializeField] private Inventory _inventory;
    private Grid _grid;
    private PlayerMovementController _playerMovementController;
    public Cursor _activeCursor;
    private List<Action> _unsubscribeHooks = new();
    private static readonly List<string> INTERACTABLE_TILEMAP_LAYERS = new List<string> { "Water" };

    private void OnEnable()
    {
        _activeCursor = _cursorE;
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
        _unsubscribeHooks.Add(_playerMovementController.FacingDirection.OnChange((prev, curr) => OnDirectionChange(curr)));
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        foreach (var hook in _unsubscribeHooks)
            hook();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _grid = GameObject.FindObjectOfType<Grid>();
    }

    private void OnDirectionChange(FacingDirection curr)
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

    public Vector3Int GetActiveCursorLocation()
    {
        if (_grid == null)
            Debug.LogError("Grid is null, can't find active cursor location");
        return _grid.WorldToCell(_activeCursor.transform.position);
    }

    private void OnUseTool()
    {
        Debug.Log("OnUseTool");
        // can't interrupt these
        if (_playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Celebrating ||
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Catching ||
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Axing ||
            _playerMovementController.PlayerState.Value == PlayerMovementController.PlayerStates.Birding)
        {
            Debug.Log("Activegridcell returned");
            return;
        }

        // check active inventory slot for tool
        Inventory.ItemType _activeItem = _inventory.GetActiveItemType();
        if (_activeItem == null)
        {
            Debug.Log("Active item is null");
            return;
        }
        ITool _activeTool = _activeItem as ITool;
        if (_activeTool == null)
        {
            Debug.Log("Active item is not ITool");
            return;
        }

        if (!PlayerCondition.Instance.IsEnergyAvailable())
        {
            Debug.Log("No energy remaining.");
            return;
        }

        // try to use tool on worldobject
        Vector3Int _cursorLocation = GetActiveCursorLocation();
        IInteractable _interactableWorldObject = FindPlayerCursorInteractableObject(_cursorLocation);
        if (_interactableWorldObject != null)
        {
            if (_activeTool.UseToolOnWorldObject(_interactableWorldObject, _cursorLocation))
            {
                PlayerCondition.Instance.DepleteEnergy(_activeTool.EnergyCost);
                _activeTool.PlayToolHitSound();
                return;
            }
        }

        // try to use tool on tilemap
        string _interactableTilemapName = FindPlayerCursorInteractableTileMap(_cursorLocation);
        if (_interactableTilemapName != null)
        {
            if (_activeTool.UseToolOnInteractableTileMap(_interactableTilemapName, _cursorLocation))
            {
                PlayerCondition.Instance.DepleteEnergy(_activeTool.EnergyCost);
                _activeTool.PlayToolHitSound();
                return;
            }
        }
        // swing at nothing
        if (_activeTool.UseToolWithoutTarget())
        {
            PlayerCondition.Instance.DepleteEnergy(_activeTool.EnergyCost);
        }
    }

    private void OnPlayerCursorAction()
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

        // check active inventory slot for interactable item
        Inventory.ItemType _activeItem = _inventory.GetActiveItemType();
        if (_activeItem == null) return;
        if (_activeItem is not IPlayerCursorUsingItem) return;

        // try to use item on worldobject
        if (_interactableWorldObject != null)
            if (((IPlayerCursorUsingItem)_activeItem).UseItemOnWorldObject(_interactableWorldObject, _cursorLocation))
                return;

        // try to use item on tilemap
        string _interactableTilemapName = FindPlayerCursorInteractableTileMap(_cursorLocation);
        if (_interactableTilemapName != null)
            if (((IPlayerCursorUsingItem)_activeItem).UseItemOnInteractableTileMap(_interactableTilemapName, _cursorLocation))
                return;
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
                Debug.LogError("There are two interactable objects on this cursor location");
                return null;
        }
    }

    private string FindPlayerCursorInteractableTileMap(Vector3Int cursorLocation)
    {
        List<string> _foundInteractableLayers = new();
        Tilemap[] _tilemaps = FindObjectsOfType<Tilemap>();

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
