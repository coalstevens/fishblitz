using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap), typeof(TilemapRenderer))]
public class WorldObjectOccupancyMap : MonoBehaviour
{
    [SerializeField] private Tile _marker;
    [SerializeField] private TilemapQuery _tilemapQuery;
    private Tilemap _tileMap;

    public bool CheckOccupied(Vector2 worldPosition)
    {
        if (_tileMap == null)
            _tileMap = GetComponent<Tilemap>();

        return CheckOccupied(_tileMap.WorldToCell(worldPosition));
    }
    public bool CheckOccupied(Vector3Int tilePosition)
    {
        if (_tileMap == null)
            _tileMap = GetComponent<Tilemap>();

        if (_tileMap.GetTile(tilePosition) != null)
            return true;

        return _tilemapQuery != null && !_tilemapQuery.IsOccupiable(tilePosition);
    }

    // Mark an area as occupied and return a callback to erase it
    public Action SetOccupied(Vector2 worldPosition, int tilesToLeft, int tilesToRight, int tilesAbove, int tilesBelow)
    {
        if(_tileMap == null) 
            _tileMap = GetComponent<Tilemap>();

        Vector3Int centerTilePos = _tileMap.WorldToCell(worldPosition);
        HashSet<Vector3Int> placedTiles = new HashSet<Vector3Int>();

        for (int x = -tilesToLeft; x <= tilesToRight; x++)
        {
            for (int y = -tilesBelow; y <= tilesAbove; y++)
            {
                Vector3Int tilePos = centerTilePos + new Vector3Int(x, y, 0);
                _tileMap.SetTile(tilePos, _marker);
                placedTiles.Add(tilePos);
            }
        }

        return () =>
        {
            foreach (var tilePos in placedTiles)
            {
                if (_tileMap != null)
                {
                    _tileMap.SetTile(tilePos, null);
                }
            }
        };
    }
}