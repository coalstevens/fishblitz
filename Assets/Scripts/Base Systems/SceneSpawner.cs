using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneSpawner : MonoBehaviour
{
    [Serializable]
    private class SpawnObjectInfo
    {
        public List<GameObject> PrefabVariants;
        public List<Tilemap> SpawnAreas = new();
        public List<TileBase> ValidTileTypes = new();
        public float Density = 0.05f;
        public bool SpawnNearbyOtherTypes = false;
        public int NearbyAdjacentDistance = 1;
        public List<GameObject> NearbyTypes = new();
        public bool UsePerlinNoise = false;
        public float PerlinNoiseScale = 0.1f;
        public float PerlinThreshold = 0.5f;
    }

    [SerializeField] private List<SpawnObjectInfo> ObjectsToSpawn = new();
    [SerializeField] private Logger _logger = new();
    private Dictionary<GameObject, List<Vector3Int>> _spawnedObjects = new();
    private Transform _impermanentContainer;
    private WorldObjectOccupancyMap _occupancyMap;
    private List<Tilemap> _allTilemaps;

    private void OnEnable()
    {
        SceneSaveLoadManager.FirstVisitToScene += SpawnObjects;
        foreach (var child in transform)
            (child as GameObject)?.SetActive(false);
    }

    private void OnDisable()
    {
        SceneSaveLoadManager.FirstVisitToScene -= SpawnObjects;
    }

    void InitializeSpawner()
    {
        _impermanentContainer = GameObject.FindGameObjectWithTag("Impermanent")?.transform;
        if (_impermanentContainer == null)
            Debug.LogError("Impermanent container not found!");
        _occupancyMap = _impermanentContainer.GetComponent<WorldObjectOccupancyMap>();
        if (_occupancyMap == null)
            Debug.LogError("Occupancy map not found!");
        _allTilemaps = new List<Tilemap>(FindObjectsByType<Tilemap>(FindObjectsSortMode.None));
        foreach (var child in transform)
            (child as GameObject)?.SetActive(true);
        VerifyVariantsAreSameSize();
    }

    private void SpawnObjects(string sceneName)
    {
        _logger.Info("Spawn start");
        InitializeSpawner();

        foreach (var spawnObject in ObjectsToSpawn)
        {
            OccupyWorldObjectSpace spaceOccupiedByObject = spawnObject.PrefabVariants[0].GetComponent<OccupyWorldObjectSpace>();

            foreach (var area in spawnObject.SpawnAreas)
            {
                List<Vector3Int> validSpawnPositions = GetPositionsOfValidTilesInArea(area, spawnObject.ValidTileTypes);
                int numToSpawn = Mathf.CeilToInt(validSpawnPositions.Count * spawnObject.Density);
                _logger.Info($"Attempting to spawn {numToSpawn} of variants of {spawnObject.PrefabVariants[0].name}");

                validSpawnPositions = RemovePositionsThatAreOccupied(validSpawnPositions);
                validSpawnPositions = RemovePositionsThatAreTooSmall(validSpawnPositions, spaceOccupiedByObject);
                if (spawnObject.UsePerlinNoise)
                    validSpawnPositions = ApplyPerlinNoiseToPositions(validSpawnPositions, area, spawnObject);
                if (spawnObject.SpawnNearbyOtherTypes)
                    validSpawnPositions = RemovePositionsNotNearTypes(validSpawnPositions, spawnObject.NearbyTypes, area, spawnObject.NearbyAdjacentDistance);

                validSpawnPositions = Shuffle(validSpawnPositions);
                while (numToSpawn > 0 && validSpawnPositions.Count > 0)
                {
                    Vector3Int tilePos = validSpawnPositions[0];

                    var variant = spawnObject.PrefabVariants[UnityEngine.Random.Range(0, spawnObject.PrefabVariants.Count)];
                    Vector3 worldPos = CalculateWorldPosition(area, tilePos, variant);
                    GameObject spawnedObj = Instantiate(variant, worldPos, Quaternion.identity, _impermanentContainer);

                    if (!_spawnedObjects.TryGetValue(variant, out var positions))
                    {
                        positions = new List<Vector3Int>();
                        _spawnedObjects[variant] = positions;
                    }
                    positions.Add(tilePos);

                    validSpawnPositions = RemovePositionsOccupiedByNewObject(validSpawnPositions, tilePos, spaceOccupiedByObject);
                    numToSpawn--;
                }
            }
        }
        gameObject.SetActive(false);
    }

    private List<Vector3Int> ApplyPerlinNoiseToPositions(List<Vector3Int> positions, Tilemap area, SpawnObjectInfo spawnObject)
    {
        List<Vector3Int> sortedPositions = new List<Vector3Int>();

        foreach (var pos in positions)
        {
            float perlinValue = Mathf.PerlinNoise(pos.x * spawnObject.PerlinNoiseScale, pos.y * spawnObject.PerlinNoiseScale);

            if (perlinValue > spawnObject.PerlinThreshold)
                sortedPositions.Add(pos);
        }

        _logger.Info($"Valid positions after Perlin noise filtering: {sortedPositions.Count}");
        return sortedPositions;
    }

    private List<Vector3Int> RemovePositionsOccupiedByNewObject(List<Vector3Int> validSpawnPositions, Vector3Int tilePos, OccupyWorldObjectSpace spaceOccupiedByObject)
    {
        if (spaceOccupiedByObject == null)
        {
            Debug.LogWarning("Object does not have an OccupyWorldObjectSpace component.");
            return validSpawnPositions;
        }

        HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();

        for (int x = -spaceOccupiedByObject.extraTilesToLeft; x <= spaceOccupiedByObject.extraTilesToRight; x++)
            for (int y = -spaceOccupiedByObject.extraTilesBelow; y <= spaceOccupiedByObject.extraTilesAbove; y++)
                occupiedPositions.Add(new Vector3Int(tilePos.x + x, tilePos.y + y, tilePos.z));

        validSpawnPositions.RemoveAll(pos => occupiedPositions.Contains(pos));
        return validSpawnPositions;
    }

    private List<Vector3Int> RemovePositionsThatAreOccupied(List<Vector3Int> validSpawnPositions)
    {
        if (_occupancyMap == null)
        {
            Debug.LogError("Occupancy map is missing!");
            return validSpawnPositions;
        }

        List<Vector3Int> filteredTiles = new List<Vector3Int>();

        foreach (var pos in validSpawnPositions)
            if (!_occupancyMap.CheckOccupied(pos))
                filteredTiles.Add(pos);

        _logger.Info($"Number of spawn positions after removing occupied spaces: {filteredTiles.Count}");
        return filteredTiles;
    }

    private List<Vector3Int> RemovePositionsThatAreTooSmall(List<Vector3Int> spawnPositions, OccupyWorldObjectSpace spaceOccupiedByObject)
    {
        if (spaceOccupiedByObject == null)
        {
            Debug.LogWarning("Object does not have an OccupyWorldObjectSpace component.");
            return spawnPositions;
        }

        List<Vector3Int> filteredTiles = new List<Vector3Int>();
        HashSet<Vector3Int> positionSet = new HashSet<Vector3Int>(spawnPositions);

        foreach (var pos in spawnPositions)
        {
            bool hasSpace = true;

            for (int x = -spaceOccupiedByObject.extraTilesToLeft; x <= spaceOccupiedByObject.extraTilesToRight; x++)
            {
                for (int y = -spaceOccupiedByObject.extraTilesBelow; y <= spaceOccupiedByObject.extraTilesAbove; y++)
                {
                    Vector3Int checkPos = new Vector3Int(pos.x + x, pos.y + y, pos.z);
                    if (!positionSet.Contains(checkPos))
                    {
                        hasSpace = false;
                        break;
                    }
                }
                if (!hasSpace) break;
            }

            if (hasSpace)
                filteredTiles.Add(pos);
        }
        _logger.Info($"Number of spawn positions after filtering by object size: {filteredTiles.Count}");
        return filteredTiles;
    }

    private List<Vector3Int> GetPositionsOfValidTilesInArea(Tilemap targetTilemap, List<TileBase> validTiles)
    {
        List<Vector3Int> validPositions = new List<Vector3Int>();
        BoundsInt bounds = targetTilemap.cellBounds;
        HashSet<TileBase> validTileSet = new HashSet<TileBase>(validTiles);

        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (!targetTilemap.HasTile(position))
                continue;

            bool isValid = false;
            foreach (var tilemap in _allTilemaps)
            {
                TileBase tile = tilemap.GetTile(position);
                if (tile != null && validTileSet.Contains(tile))
                {
                    isValid = true;
                    break;
                }
            }

            if (isValid)
                validPositions.Add(position);
        }

        _logger.Info($"Valid tiles in area: {validPositions.Count}");
        return validPositions;
    }

    private List<Vector3Int> RemovePositionsNotNearTypes(List<Vector3Int> spawnPositions, List<GameObject> nearTypes, Tilemap tilemap, int adjacentDistance)
    {
        if (nearTypes == null || nearTypes.Count == 0)
            return spawnPositions;

        HashSet<Vector3Int> nearbyPositions = new HashSet<Vector3Int>();

        foreach (var nearType in nearTypes)
        {
            if (_spawnedObjects.TryGetValue(nearType, out var spawnedPositions))
            {
                foreach (var pos in spawnedPositions)
                    nearbyPositions.Add(pos);
            }
        }

        GameObject[] existingObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        HashSet<GameObject> nearTypeSet = new HashSet<GameObject>(nearTypes);
        foreach (var obj in existingObjects)
        {
            if (nearTypeSet.Contains(obj))
            {
                Vector3Int objTilePos = tilemap.WorldToCell(obj.transform.position);
                nearbyPositions.Add(objTilePos);
            }
        }

        List<Vector3Int> filteredPositions = new List<Vector3Int>();

        foreach (var pos in spawnPositions)
        {
            bool foundNearby = false;

            for (int x = -adjacentDistance; x <= adjacentDistance && !foundNearby; x++)
            {
                for (int y = -adjacentDistance; y <= adjacentDistance && !foundNearby; y++)
                {
                    Vector3Int checkPos = new Vector3Int(pos.x + x, pos.y + y, pos.z);
                    if (nearbyPositions.Contains(checkPos))
                        foundNearby = true;
                }
            }

            if (foundNearby)
                filteredPositions.Add(pos);
        }

        _logger.Info($"Number of spawn positions after proximity filtering: {filteredPositions.Count}");
        return filteredPositions;
    }

    private List<T> Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    private void VerifyVariantsAreSameSize()
    {
        if (ObjectsToSpawn == null || ObjectsToSpawn.Count <= 1)
            return;

        foreach (var spawnObject in ObjectsToSpawn)
        {
            if (spawnObject.PrefabVariants == null || spawnObject.PrefabVariants.Count == 0)
                continue;

            OccupyWorldObjectSpace initialSpaceOccupied = spawnObject.PrefabVariants[0].GetComponent<OccupyWorldObjectSpace>();

            foreach (var variant in spawnObject.PrefabVariants)
            {
                OccupyWorldObjectSpace variantSpace = variant.GetComponent<OccupyWorldObjectSpace>();

                if (variantSpace == null)
                {
                    Debug.LogError($"Prefab {variant.name} does not have an OccupyWorldObjectSpace component.");
                    continue;
                }

                if (initialSpaceOccupied.extraTilesToLeft != variantSpace.extraTilesToLeft ||
                    initialSpaceOccupied.extraTilesToRight != variantSpace.extraTilesToRight ||
                    initialSpaceOccupied.extraTilesBelow != variantSpace.extraTilesBelow ||
                    initialSpaceOccupied.extraTilesAbove != variantSpace.extraTilesAbove)
                {
                    Debug.LogError($"Prefab variants for {variant.name} do not occupy the same space.");
                    return;
                }
            }
        }
    }

    public static Vector3 CalculateWorldPosition(Grid grid, Vector3Int cellPosition, GameObject prefab = null)
    {
        Vector3 position = grid.CellToWorld(cellPosition) + new Vector3(0.5f, 0, 0);

        if (prefab != null)
        {
            SpawnOffset offset = prefab.GetComponent<SpawnOffset>();
            if (offset != null)
                position += (Vector3)offset.Offset;
        }

        return position;
    }

    public static Vector3 CalculateWorldPosition(Tilemap tilemap, Vector3Int cellPosition, GameObject prefab = null)
    {
        Vector3 position = tilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0, 0);

        if (prefab != null)
        {
            SpawnOffset offset = prefab.GetComponent<SpawnOffset>();
            if (offset != null)
                position += (Vector3)offset.Offset;
        }

        return position;
    }
}
