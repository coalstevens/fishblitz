using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ActiveGridCell : MonoBehaviour
{
    [SerializeField] private Cursor _cursorN;
    [SerializeField] private Cursor _cursorE;
    [SerializeField] private Cursor _cursorS;
    [SerializeField] private Cursor _cursorW;
    [SerializeField] private PlayerMovementController _playerMovementController;
    [SerializeField] private Grid _grid;
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private Inventory _inventory;

    private Cursor _activeCursor;

    // Start is called before the first frame update
    void Start()
    {
        _playerMovementController.FacingDir.OnChange((prev, curr) => OnDirectionChange(curr));
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

    private void OnFire()
    {
        // no placements when ur fishing
        if (_playerMovementController.Fishing)
        {
            return;
        }

        FishingRod rod = GetRod();
        if (rod != null)
        {
            rod.StartFishingGame();
        }
        else
        {
            PlaceRod();
        }
    }

    private void PlaceRod()
    {
        TileBase tile = _tilemap.GetTile(GetActiveCursorLocation());
        TileData tileData = new TileData();
        tile.GetTileData(GetActiveCursorLocation(), _tilemap, ref tileData);

        bool canBePlaced = tileData.gameObject != null;
        bool hasRod = _inventory.Rods > 0;

        if (canBePlaced && hasRod)
        {
            Instantiate(tileData.gameObject.GetComponent<RodPlacement>().RodToPlace, (Vector3)GetActiveCursorLocation() + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
            _inventory.Rods -= 1;
        }
    }

    private FishingRod GetRod()
    {
        List<Collider2D> results = new List<Collider2D>();
        Physics2D.OverlapBox(GetActiveCursorLocation() + new Vector3(0.5f, 0.5f, 0f), new Vector2(1, 1), 0, new ContactFilter2D().NoFilter(), results);

        foreach (var result in results)
        {
            var rod = result.GetComponent<FishingRod>();
            if (rod != null)
            {
                return rod;
            }
        }

        return null;
    }
}
