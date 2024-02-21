using System.Collections;
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
    Tilemap[] _tilemaps;
    private Grid _grid;
    private PlayerMovementController _playerMovementController;
    private Inventory _inventory;
    public Cursor _activeCursor;    

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        _activeCursor = _cursorE;
        _tilemaps = FindObjectsOfType<Tilemap>();
        _playerMovementController = GameObject.FindWithTag("Player").GetComponent<PlayerMovementController>();
        _playerMovementController.FacingDir.OnChange((prev, curr) => OnDirectionChange(curr));
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
        _grid = GameObject.FindObjectOfType<Grid>();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        _grid = GameObject.FindObjectOfType<Grid>();
    }
    private void OnDirectionChange(Direction curr)
    {
        switch (curr)
        {
            case Direction.Up:
                _activeCursor = _cursorN;
                return;
            case Direction.Right:
                _activeCursor = _cursorE;
                return;
            case Direction.Down:
                _activeCursor = _cursorS;
                return;
            case Direction.Left:
                _activeCursor = _cursorW;
                return;
        }
    }

    public Vector3Int GetActiveCursorLocation()
    {
        return _grid.WorldToCell(_activeCursor.transform.position);
    }

    private void OnUseTool() {
        // no interrupting celebrating
        if (_playerMovementController.CurrState.Value == State.Celebrating)
        {
            return;
        }
        
        // no interrupting catching
        if (_playerMovementController.CurrState.Value == State.Catching)
        {
            return;
        }
        
        // return if empty item slot selected
        IInventoryItem _activeItem = _inventory.GetActiveItem();
        if (_activeItem == null) 
        {
            return;
        }

        // return if item is not a tool
        if (_activeItem is not ITool) {
            return;
        }
    
        Vector3Int _cursorLocation = GetActiveCursorLocation();
        IInteractable _interactableWorldObject = FindCursorInteractableObject(_cursorLocation);
        if (_interactableWorldObject != null) {
            // use the tool on the world object
            ((ITool)_activeItem).UseToolOnWorldObject(_interactableWorldObject, _cursorLocation);
            return;
        }
        else {
            // use the tool on the tile
            IInteractableTile _interactableTile = FindCursorInteractableTile(_cursorLocation);
            if (_interactableTile != null) {
                ((ITool)_activeItem).UseToolOnTile(_interactableTile,_cursorLocation);
                return;
            }
        }
    }

    private void OnCursorAction()
    {
        // no action when the player is not idle or walking
        if (_playerMovementController.CurrState.Value != State.Idle && _playerMovementController.CurrState.Value != State.Walking )
        {
            return;
        }

        Vector3Int _cursorLocation = GetActiveCursorLocation();
        
        // interact with the object the cursor is on, if any
        IInteractable _interactableWorldObject = FindCursorInteractableObject(_cursorLocation);
        if (_interactableWorldObject != null) {
            if (_interactableWorldObject.CursorInteract(_cursorLocation)) {
                return;
            }
        }

        // return if empty item slot selected
        IInventoryItem _activeItem = _inventory.GetActiveItem();
        if (_activeItem == null) 
        {
            return;
        }

        // return if item doesn't use the cursor
        if (_activeItem is not IPlayerCursorUsingItem) {
            return;
        }

        if (_interactableWorldObject != null) {
            // interact the item on the world object
            ((IPlayerCursorUsingItem)_activeItem).UseItemOnWorldObject(_interactableWorldObject, _cursorLocation);
            return;
        }
        else {
            // interact the item on the tile
            IInteractableTile _interactableTile = FindCursorInteractableTile(_cursorLocation);
            if (_interactableTile != null) {
                ((IPlayerCursorUsingItem)_activeItem).UseItemOnTile(_interactableTile,_cursorLocation);
                return;
            }
        }
    }

    private IInteractable FindCursorInteractableObject(Vector3Int cursorLocation)
    {
        List<Collider2D> _results = new List<Collider2D>();
        List<IInteractable> _foundInteractables = new List<IInteractable>();
        Physics2D.OverlapBox(cursorLocation + new Vector3(0.5f, 0.5f, 0f), new Vector2(1, 1), 0, new ContactFilter2D().NoFilter(), _results);
        foreach (var _result in _results)
        {
            IInteractable _currentObject = _result.GetComponent<IInteractable>();
            if (_currentObject != null)
            {
                _foundInteractables.Add(_currentObject);
            }
        }

        switch (_foundInteractables.Count) {
            case 1: 
                return _foundInteractables[0];
            case 0: 
                return null;
            default: 
                Debug.LogError("There are two interactable objects on this cursor location");
                return null;
        }
    }

    private IInteractableTile FindCursorInteractableTile(Vector3Int cursorLocation) {
        List<IInteractableTile> _foundInteractables = new List<IInteractableTile>();
        TileData _tileData = new TileData();

        foreach (Tilemap _tilemap in _tilemaps)
        {
            if (IsWorldPositionInTilemap(_tilemap, cursorLocation))
            {
                TileBase _tile = _tilemap.GetTile(cursorLocation);

                if (_tile != null) {
                    _tile.GetTileData(cursorLocation, _tilemap, ref _tileData);
                } 

                IInteractableTile _currentInteractable = _tileData.gameObject.GetComponent<IInteractableTile>();

                if (_currentInteractable != null) {
                    _foundInteractables.Add(_currentInteractable);
                }
            }
        }
        switch (_foundInteractables.Count) {
            case 1: 
                return _foundInteractables[0];
            case 0: 
                return null;
            default: 
                Debug.LogError("There are two interactable tiles on this cursor location");
                return null;
        }
    }
    private bool IsWorldPositionInTilemap(Tilemap tilemap, Vector3 worldPosition)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        return tilemap.cellBounds.Contains(cellPosition);
    }
}
