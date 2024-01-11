using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public interface IInteractableWorldObject {
    /// <summary>
    /// Returns true if an action is completed, returns false if the command is ignored 
    /// </summary>
    public bool CursorAction(TileData tileData, Vector3 cursorLocation);
}

public class ActiveGridCell : MonoBehaviour
{
    [SerializeField] private Cursor _cursorN;
    [SerializeField] private Cursor _cursorE;
    [SerializeField] private Cursor _cursorS;
    [SerializeField] private Cursor _cursorW;
    [SerializeField] private Grid _grid;
    [SerializeField] private Tilemap _tilemap;
    private PlayerMovementController _playerMovementController;
    private Inventory _inventory;
    public Cursor _activeCursor;    

    void Start()
    {
        _playerMovementController = GameObject.FindWithTag("Player").GetComponent<PlayerMovementController>();
        _playerMovementController.FacingDir.OnChange((prev, curr) => OnDirectionChange(curr));
        _inventory = GameObject.FindWithTag("Inventory").GetComponent<Inventory>();
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
        TileBase _tile = _tilemap.GetTile(_cursorLocation);
        TileData _tileData = new TileData();
        if (_tile != null) {
            _tile.GetTileData(_cursorLocation, _tilemap, ref _tileData);
        }


        ((ITool)_activeItem).UseTool(_tileData,_cursorLocation);
    }

    private void OnCursorAction()
    {
        // no action when the player is not idle or walking
        if (_playerMovementController.CurrState.Value != State.Idle && _playerMovementController.CurrState.Value != State.Walking )
        {
            return;
        }
        Vector3Int _cursorLocation = GetActiveCursorLocation();

        //TODO what if there isn't a tile?
        TileBase _tile = _tilemap.GetTile(_cursorLocation);
        TileData _tileData = new TileData();
        if (_tile != null) {
            _tile.GetTileData(_cursorLocation, _tilemap, ref _tileData);
        }
        
        // interact with the object the cursor is on
        // CursorAction
        IInteractableWorldObject _cursorInteractableObject = GetCursorInteractableObject();
        if (_cursorInteractableObject != null) {
            if (_cursorInteractableObject.CursorAction(_tileData,_cursorLocation)) {
                return;
            }
        }

        // reutrn if empty item slot selected
        IInventoryItem _activeItem = _inventory.GetActiveItem();
        if (_activeItem == null) 
        {
            return;
        }

        // return if item doesn't use the cursor
        if (_activeItem is not IPlayerCursorUsingItem) {
            return;
        }

        ((IPlayerCursorUsingItem)_activeItem).CursorAction(_tileData,_cursorLocation);
    }

    private IInteractableWorldObject GetCursorInteractableObject()
    {
        List<Collider2D> _results = new List<Collider2D>();
        Physics2D.OverlapBox(GetActiveCursorLocation() + new Vector3(0.5f, 0.5f, 0f), new Vector2(1, 1), 0, new ContactFilter2D().NoFilter(), _results);

        foreach (var _result in _results)
        {
            var _placedItem = _result.GetComponent<IInteractableWorldObject>();
            if (_placedItem != null)
            {
                return _placedItem;
            }
        }
        return null;
    }
}
