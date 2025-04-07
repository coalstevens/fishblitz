using System;
using UnityEngine;

public class OccupyWorldObjectSpace : MonoBehaviour
{
    public int extraTilesToLeft = 0;
    public int extraTilesToRight = 0;
    public int extraTilesAbove = 0;
    public int extraTilesBelow = 0;
    private WorldObjectOccupancyMap _occupancyMap;
    private Action _removeOccupancy;

    private void OnEnable()
    {
        _occupancyMap = GameObject.FindGameObjectWithTag("Impermanent")?.GetComponent<WorldObjectOccupancyMap>();
        if (_occupancyMap == null)
            Debug.LogError("There is no occupancy map in this scene");
        _removeOccupancy = _occupancyMap.SetOccupied(transform.position, extraTilesToLeft, extraTilesToRight, extraTilesAbove, extraTilesBelow);
    }

    private void OnDisable()
    {
        _removeOccupancy?.Invoke();
    }
}
