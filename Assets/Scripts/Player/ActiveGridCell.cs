using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class ActiveGridCell : MonoBehaviour
{
    [SerializeField] private Cursor _cursorN;
    [SerializeField] private Cursor _cursorE;
    [SerializeField] private Cursor _cursorS;
    [SerializeField] private Cursor _cursorW;
    private Grid _grid;
    private PlayerMovementController _playerMovementController;
    private Inventory _inventory;
    public Cursor _activeCursor;
    private List<Action> _unsubscribeHooks = new();
    private static readonly List<string> INTERACTABLE_TILEMAP_LAYERS = new List<string> { "Water" };

    private void OnEnable()
    {
        _activeCursor = _cursorE;
        _playerMovementController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementController>();
        _inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
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
        if (_grid != null)
        {
            // Debug.Log("Found the grid");
        }
        else
        {
            // Debug.Log("Can't find a grid");
        }
    }

    private void OnDirectionChange(FacingDirections curr)
    {
        switch (curr)
        {
            case FacingDirections.North:
                _activeCursor = _cursorN;
                return;
            case FacingDirections.East:
                _activeCursor = _cursorE;
                return;
            case FacingDirections.South:
                _activeCursor = _cursorS;
                return;
            case FacingDirections.West:
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
        // can't interrupt these
        if (_playerMovementController.PlayerState.Value == PlayerStates.Celebrating ||
            _playerMovementController.PlayerState.Value == PlayerStates.Catching ||
            _playerMovementController.PlayerState.Value == PlayerStates.Axing)
            return;

        // check active inventory slot for tool
        IInventoryItem _activeItem = _inventory.GetActiveItem();
        if (_activeItem == null) return;
        if (_activeItem is not ITool) return;

        // try to use tool on worldobject
        Vector3Int _cursorLocation = GetActiveCursorLocation();
        IInteractable _interactableWorldObject = FindPlayerCursorInteractableObject(_cursorLocation);
        if (_interactableWorldObject != null)
            if (((ITool)_activeItem).UseToolOnWorldObject(_interactableWorldObject, _cursorLocation)) {
                ((ITool)_activeItem).PlayToolHitSound();
                return;
            }

        // try to use tool on tilemap
        string _interactableTilemapName = FindPlayerCursorInteractableTileMap(_cursorLocation);
        if (_interactableTilemapName != null)
            if (((ITool)_activeItem).UseToolOnInteractableTileMap(_interactableTilemapName, _cursorLocation)) {
                ((ITool)_activeItem).PlayToolHitSound();
                return;
            }

        // swing at nothing
        ((ITool)_activeItem).SwingTool();
    }

    private void OnPlayerCursorAction()
    {
        // returns if player is not idle or walking
        if (_playerMovementController.PlayerState.Value != PlayerStates.Idle &&
            _playerMovementController.PlayerState.Value != PlayerStates.Walking)
            return;

        // Check for an interactable object
        Vector3Int _cursorLocation = GetActiveCursorLocation();
        IInteractable _interactableWorldObject = FindPlayerCursorInteractableObject(_cursorLocation);
        if (_interactableWorldObject?.CursorInteract(_cursorLocation) == true)
            return;

        // check active inventory slot for interactable item
        IInventoryItem _activeItem = _inventory.GetActiveItem();
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
