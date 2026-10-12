using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewTilemapQuery", menuName = "Tilemaps/Tilemap Query")]
public class TilemapQuery : ScriptableObject
{
    public List<TileSet> OccupiableTileSets = new();

    private HashSet<TileBase> _occupiableSet;
    private List<Tilemap> _worldTilemaps;

    private readonly HashSet<string> _reportedBlockers = new();

    public bool IsOccupiable(Vector3Int cellPosition)
    {
        TileBase topTile = GetTopTile(cellPosition, out Tilemap winningTilemap);
        if (topTile == null)
        {
            ReportBlockerOnce(winningTilemap, null);
            return false;
        }

        bool occupiable = OccupiableSet.Contains(topTile);
        if (!occupiable)
            ReportBlockerOnce(winningTilemap, topTile);
        return occupiable;
    }

    private TileBase GetTopTile(Vector3Int cellPosition, out Tilemap winningTilemap)
    {
        TileBase winner = null;
        winningTilemap = null;
        int highestLayer = int.MinValue;
        int highestOrder = int.MinValue;

        foreach (Tilemap tilemap in GetWorldTilemaps())
        {
            TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
            if (renderer == null || !renderer.enabled) continue;

            TileBase tile = tilemap.GetTile(cellPosition);
            if (tile == null) continue;

            int layer = SortingLayer.GetLayerValueFromID(renderer.sortingLayerID);
            int order = renderer.sortingOrder;

            if (layer > highestLayer || (layer == highestLayer && order > highestOrder))
            {
                highestLayer = layer;
                highestOrder = order;
                winner = tile;
                winningTilemap = tilemap;
            }
        }

        return winner;
    }

    private void ReportBlockerOnce(Tilemap tilemap, TileBase tile)
    {
        string source = tilemap != null ? tilemap.name : "(no tilemap)";
        string tileName = tile != null ? tile.name : "(empty)";
        if (_reportedBlockers.Add($"{source}:{tileName}"))
            Debug.Log($"TilemapQuery blocking '{tileName}' on '{source}'");
    }

    private HashSet<TileBase> OccupiableSet
    {
        get
        {
            if (_occupiableSet == null)
                RebuildOccupiableSet();
            return _occupiableSet;
        }
    }

    private void RebuildOccupiableSet()
    {
        _occupiableSet = new HashSet<TileBase>();
        if (OccupiableTileSets == null) return;

        foreach (TileSet tileSet in OccupiableTileSets)
        {
            if (tileSet == null || tileSet.Tiles == null) continue;
            foreach (TileBase tile in tileSet.Tiles)
                if (tile != null)
                    _occupiableSet.Add(tile);
        }
    }

    private List<Tilemap> GetWorldTilemaps()
    {
        if (_worldTilemaps == null)
        {
            _worldTilemaps = new List<Tilemap>();
            GameObject tilemapsContainer = GameObject.FindGameObjectWithTag("Tilemaps");
            if (tilemapsContainer != null)
                _worldTilemaps.AddRange(tilemapsContainer.GetComponentsInChildren<Tilemap>());
        }
        return _worldTilemaps;
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene current, Scene next)
    {
        _worldTilemaps = null;
        _reportedBlockers.Clear();
    }

    private void OnValidate()
    {
        _occupiableSet = null;
        _reportedBlockers.Clear();
    }
}