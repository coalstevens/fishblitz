using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class GridCursor : MonoBehaviour
{
    [SerializeField] private PlayerCursor _cursorN;
    [SerializeField] private PlayerCursor _cursorE;
    [SerializeField] private PlayerCursor _cursorS;
    [SerializeField] private PlayerCursor _cursorW;
    private PlayerCursor _activeCursor;
    private Grid _grid;
    private List<Action> _unsubscribeHooks = new();
    private PlayerMovementController _playerMovementController;
    public Vector3Int GridPosition => _grid.WorldToCell(_activeCursor.transform.position);
    public Collider2D Collider => _activeCursor.Collider;

    private void OnEnable()
    {
        _activeCursor = _cursorE;
        _playerMovementController = transform.parent.GetComponent<PlayerMovementController>();
        _grid = GameObject.FindFirstObjectByType<Grid>();
        Assert.IsNotNull(_playerMovementController);
        Assert.IsNotNull(_grid);

        _unsubscribeHooks.Add(_playerMovementController.FacingDirection.OnChange((prev, curr) => SetPlayerCursorToFacingDirection(curr)));
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= (scene, mode) => _grid = GameObject.FindFirstObjectByType<Grid>();
        foreach (var hook in _unsubscribeHooks)
            hook();
        _unsubscribeHooks.Clear();
    }

    private void SetPlayerCursorToFacingDirection(CompassDirection curr)
    {
        switch (curr)
        {
            case CompassDirection.North:
                _activeCursor = _cursorN;
                return;
            case CompassDirection.East:
                _activeCursor = _cursorE;
                return;
            case CompassDirection.South:
                _activeCursor = _cursorS;
                return;
            case CompassDirection.West:
                _activeCursor = _cursorW;
                return;
        }
    }

    public T FindObjectAtGridCursor<T>() where T : class
    {
        List<Collider2D> _results = new List<Collider2D>();
        List<T> _foundObjects = new List<T>();

        Physics2D.OverlapCollider(Collider, new ContactFilter2D().NoFilter(), _results);

        foreach (var _result in _results)
        {
            T _currentObject = _result.GetComponent<T>();
            if (_currentObject != null)
                _foundObjects.Add(_currentObject);
        }

        if (_foundObjects.Count == 0)
            return null;
        if (_foundObjects.Count > 1)
            Debug.LogWarning($"There are {_foundObjects.Count} objects of type {typeof(T).Name} on this cursor location.");

        return _foundObjects[0];
    }

    public string FindInteractableTileMapByTags(List<string> tilemapTags)
    {
        List<string> _foundInteractableTags = new();
        Tilemap[] _tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        var CursorPosition = GridPosition;

        foreach (Tilemap _tilemap in _tilemaps)
            if (IsWorldPositionInTilemap(_tilemap, CursorPosition) && tilemapTags.Contains(_tilemap.tag))
                _foundInteractableTags.Add(_tilemap.tag);

        if (_foundInteractableTags.Count == 0)
            return null;

        if (_foundInteractableTags.Count > 1)
            Debug.LogWarning($"There are {_foundInteractableTags.Count} tilemaps with matching tags on this cursor location.");
        
        return _foundInteractableTags[0];
    }

    private bool IsWorldPositionInTilemap(Tilemap tilemap, Vector3 worldPosition)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        return tilemap.GetTile(cellPosition) != null;
    }
}
