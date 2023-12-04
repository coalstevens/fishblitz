using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public interface ICursorInteractableObject {
    public void CursorAction(TileData tileData, Vector3 cursorLocation);
}
public interface ICursorUsingItem {
    public void CursorAction(TileData tileData, Vector3 cursorLocation);
}
public interface ITool {
    public void UseTool(TileData tileData, Vector3 cursorLocation);
}

public class ActiveGridCell : MonoBehaviour
{
    [SerializeField] private Cursor _cursorN;
    [SerializeField] private Cursor _cursorE;
    [SerializeField] private Cursor _cursorS;
    [SerializeField] private Cursor _cursorW;
    [SerializeField] private PlayerMovementController _playerMovementController;
    [SerializeField] private Grid _grid;
    [SerializeField] private Tilemap _tilemap;
    //[SerializeField] private Inventory _inventory;
    private InventoryController _inventory;
    private Cursor _activeCursor;    

    void Start()
    {
        _playerMovementController.FacingDir.OnChange((prev, curr) => OnDirectionChange(curr));
        _inventory = GameObject.FindWithTag("InventoryContainer").GetComponent<InventoryController>();
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
        // no interrupting cele
        if (_playerMovementController.CurrState.Value == State.Celebrating)
        {
            return;
        }
        
        // no interrupting catching
        if (_playerMovementController.CurrState.Value == State.Catching)
        {
            return;
        }

        // empty item slot selected
        GameObject _activeItem = _inventory.GetActiveItem();
        if (_activeItem == null) 
        {
            return;
        }
        
        // item is not a tool
        ITool _activeTool = _activeItem.GetComponent<ITool>();
        if (_activeTool == null) {
            return;
        }

        Vector3Int _cursorLocation = GetActiveCursorLocation();
        TileBase tile = _tilemap.GetTile(_cursorLocation);
        TileData tileData = new TileData();
        tile.GetTileData(_cursorLocation, _tilemap, ref tileData);
        _activeTool.UseTool(tileData,_cursorLocation);
    }

    private void OnCursorAction()
    {
        // no action when the player is not idle or walking
        if (_playerMovementController.CurrState.Value != State.Idle && _playerMovementController.CurrState.Value != State.Walking )
        {
            return;
        }
        Vector3Int _cursorLocation = GetActiveCursorLocation();
        TileBase _tile = _tilemap.GetTile(_cursorLocation);
        TileData _tileData = new TileData();
        _tile.GetTileData(_cursorLocation, _tilemap, ref _tileData);

        // interact with the object the cursor is on
        ICursorInteractableObject _cursorInteractableObject = GetCursorInteractableObject();
        if (_cursorInteractableObject != null) {
            _cursorInteractableObject.CursorAction(_tileData,_cursorLocation);
            return;
        }

        // empty item slot selected
        GameObject _activeItem = _inventory.GetActiveItem();
        if (_activeItem == null) 
        {
            return;
        }
        
        // item doesn't use the cursor
        ICursorUsingItem _cursorUsingItem = _activeItem.GetComponent<ICursorUsingItem>();
        if (_cursorUsingItem == null) {
            return;
        }

        _cursorUsingItem.CursorAction(_tileData,_cursorLocation);
    }

    private ICursorInteractableObject GetCursorInteractableObject()
    {
        List<Collider2D> results = new List<Collider2D>();
        Physics2D.OverlapBox(GetActiveCursorLocation() + new Vector3(0.5f, 0.5f, 0f), new Vector2(1, 1), 0, new ContactFilter2D().NoFilter(), results);

        foreach (var result in results)
        {
            var placedItem = result.GetComponent<ICursorInteractableObject>();
            if (placedItem != null)
            {
                return placedItem;
            }
        }
        return null;
    }
}
