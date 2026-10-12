using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewTileSet", menuName = "Tilemaps/Tile Set")]
public class TileSet : ScriptableObject
{
    public List<TileBase> Tiles = new();
}