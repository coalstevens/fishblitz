using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloodDamage : MonoBehaviour
{
    [SerializeField] private List<Tilemap> _damagedAreas;
    private Transform _impermanent;
    private void Start()
    {
        _impermanent = GameObject.FindGameObjectWithTag("Impermanent").transform;
        if (WorldState.WaterState.Value == WorldState.WaterStates.Flood)
            DestroyDamagedObjects();
    }

    private void DestroyDamagedObjects()
    {
        foreach (var tilemap in _damagedAreas)
        {
            foreach (Transform child in _impermanent)
            {
                Vector3Int cellPosition = tilemap.WorldToCell(child.position);
                if (tilemap.HasTile(cellPosition))
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
