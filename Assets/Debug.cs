using UnityEngine;
using UnityEngine.Tilemaps;

public class Debug : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap;

    [ContextMenu("Test")]
    private void Test()
    {
        TileBase tile = _tilemap.GetTile(new Vector3Int(-13, 1, 0));
        TileData tileData = new TileData();
        tile.GetTileData(new Vector3Int(-12, 1, 0), _tilemap, ref tileData);
        print(tileData.gameObject);
    }
}
