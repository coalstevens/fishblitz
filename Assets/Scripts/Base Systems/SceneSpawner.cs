using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneSpawner : MonoBehaviour
{
    [SerializeField] private List<SpawnObjectInfo> SpawnConfigs = new();
    [SerializeField] private Logger _logger = new();
    private Tilemap _tilemap;
    private List<Tilemap> _worldTilemaps;
    private Dictionary<GameObject, List<Vector3Int>> _spawnedObjects = new();
    private Transform _impermanentContainer;
    private WorldObjectOccupancyMap _occupancyMap;

    private void OnEnable()
    {
        SceneSaveLoadManager.FirstVisitToScene += SpawnObjects;
    }

    private void OnDisable()
    {
        SceneSaveLoadManager.FirstVisitToScene -= SpawnObjects;
    }

    void InitializeSpawner()
    {
        if (_tilemap == null)
            _tilemap = GetComponent<Tilemap>();

        GameObject tilemapsContainer = GameObject.FindGameObjectWithTag("Tilemaps");
        if (tilemapsContainer != null)
            _worldTilemaps = new List<Tilemap>(tilemapsContainer.GetComponentsInChildren<Tilemap>());
        else
            _logger.Warning("No GameObject with 'Tilemaps' tag found!");

        BoundsInt bounds = _tilemap.cellBounds;
        _logger.Info($"Marker tilemap: {_tilemap.name}, bounds: ({bounds.xMin},{bounds.yMin})-({bounds.xMax},{bounds.yMax})");
        _logger.Info($"World tilemaps found: {(_worldTilemaps?.Count ?? 0)}");

        Dictionary<string, int> tileCounts = new();
        if (_worldTilemaps != null)
        {
            foreach (var wt in _worldTilemaps)
            {
                foreach (Vector3Int pos in wt.cellBounds.allPositionsWithin)
                {
                    if (!wt.HasTile(pos)) continue;
                    TileBase tile = wt.GetTile(pos);
                    if (tile == null) continue;
                    string key = $"{tile.name} ({tile.GetType().Name}) on {wt.name}";
                    tileCounts.TryGetValue(key, out int c);
                    tileCounts[key] = c + 1;
                }
            }
        }
        List<string> tileSummary = new();
        foreach (var kv in tileCounts)
            tileSummary.Add($"{kv.Value}×{kv.Key}");
        _logger.Verbose($"World tiles: {string.Join(", ", tileSummary)}");

        int markerCount = 0;
        Dictionary<string, int> markerTileHits = new();
        foreach (Vector3Int pos in _tilemap.cellBounds.allPositionsWithin)
        {
            if (!_tilemap.HasTile(pos)) continue;
            markerCount++;
            bool found = false;
            if (_worldTilemaps != null)
            {
                foreach (var wt in _worldTilemaps)
                {
                    TileBase tile = wt.GetTile(pos);
                    if (tile == null) continue;
                    string key = $"{tile.name} on {wt.name}";
                    markerTileHits.TryGetValue(key, out int c);
                    markerTileHits[key] = c + 1;
                    found = true;
                }
            }
            if (!found)
            {
                string key = "(no world tile)";
                markerTileHits.TryGetValue(key, out int c);
                markerTileHits[key] = c + 1;
            }
        }
        List<string> markerSummary = new();
        foreach (var kv in markerTileHits)
            markerSummary.Add($"{kv.Value} {kv.Key}");
        _logger.Verbose($"Markers: {markerCount} — {string.Join(", ", markerSummary)}");

        foreach (var config in SpawnConfigs)
        {
            string names = "";
            foreach (var t in config.ValidTileTypes)
                names += (names.Length > 0 ? ", " : "") + (t != null ? t.name : "null");
            _logger.Verbose($"Config '{config.name}' looking for: [{names}]");
        }

        _impermanentContainer = GameObject.FindGameObjectWithTag("Impermanent")?.transform;
        if (_impermanentContainer == null)
            Debug.LogError("Impermanent container not found!");
        _occupancyMap = _impermanentContainer.GetComponent<WorldObjectOccupancyMap>();
        if (_occupancyMap == null)
            Debug.LogError("Occupancy map not found!");
        VerifyVariantsAreSameSize();
    }

    private void SpawnObjects(string sceneName)
    {
        _logger.Verbose("Spawn start");
        InitializeSpawner();

        foreach (var spawnConfig in SpawnConfigs)
        {
            OccupyWorldObjectSpace spaceOccupiedByObject = spawnConfig.PrefabVariants[0].GetComponent<OccupyWorldObjectSpace>();

            List<Vector3Int> validSpawnPositions = GetPositionsOfValidTilesInArea(_tilemap, spawnConfig.ValidTileTypes);
            int numToSpawn = Mathf.CeilToInt(validSpawnPositions.Count * spawnConfig.Density);

            string filterLog = $"'{spawnConfig.name}': {validSpawnPositions.Count} tile-valid";
            validSpawnPositions = RemovePositionsThatAreOccupied(validSpawnPositions);
            filterLog += $" → {validSpawnPositions.Count} unoccupied";
            validSpawnPositions = RemovePositionsThatAreTooSmall(validSpawnPositions, spaceOccupiedByObject);
            filterLog += $" → {validSpawnPositions.Count} size-ok";
            if (spawnConfig.UsePerlinNoise)
            {
                validSpawnPositions = ApplyPerlinNoiseToPositions(validSpawnPositions, spawnConfig);
                filterLog += $" → {validSpawnPositions.Count} noise-pass";
            }
            if (spawnConfig.SpawnNearbyOtherTypes)
            {
                validSpawnPositions = RemovePositionsNotNearTypes(validSpawnPositions, spawnConfig.NearbyTypes, _tilemap, spawnConfig.NearbyAdjacentDistance);
                filterLog += $" → {validSpawnPositions.Count} near-match";
            }

            validSpawnPositions = Shuffle(validSpawnPositions);
            _logger.Info($"{filterLog} → spawning {numToSpawn}");
            while (numToSpawn > 0 && validSpawnPositions.Count > 0)
            {
                Vector3Int tilePos = validSpawnPositions[0];

                var variant = spawnConfig.PrefabVariants[UnityEngine.Random.Range(0, spawnConfig.PrefabVariants.Count)];
                Vector3 worldPos = CalculateWorldPosition(_tilemap, tilePos, variant);
                Instantiate(variant, worldPos, Quaternion.identity, _impermanentContainer);

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
        gameObject.SetActive(false);
    }

    private List<Vector3Int> ApplyPerlinNoiseToPositions(List<Vector3Int> positions, SpawnObjectInfo spawnConfig)
    {
        List<Vector3Int> sortedPositions = new List<Vector3Int>();

        foreach (var pos in positions)
        {
            float perlinValue = Mathf.PerlinNoise(pos.x * spawnConfig.PerlinNoiseScale, pos.y * spawnConfig.PerlinNoiseScale);

            if (perlinValue > spawnConfig.PerlinThreshold)
                sortedPositions.Add(pos);
        }

        _logger.Verbose($"Valid positions after Perlin noise filtering: {sortedPositions.Count}");
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

        _logger.Verbose($"Number of spawn positions after removing occupied spaces: {filteredTiles.Count}");
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
        _logger.Verbose($"Number of spawn positions after filtering by object size: {filteredTiles.Count}");
        return filteredTiles;
    }

    private List<Vector3Int> GetPositionsOfValidTilesInArea(Tilemap markerTilemap, List<TileBase> validTiles)
    {
        List<Vector3Int> validPositions = new List<Vector3Int>();
        BoundsInt bounds = markerTilemap.cellBounds;
        HashSet<string> validTileNames = new HashSet<string>();
        bool allowNoTile = false;
        foreach (var t in validTiles)
        {
            if (t != null)
                validTileNames.Add(t.name);
            else
                allowNoTile = true;
        }

        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (!markerTilemap.HasTile(position))
                continue;

            bool anyWorldTile = false;
            if (_worldTilemaps != null)
            {
                foreach (var wt in _worldTilemaps)
                {
                    TileBase tile = wt.GetTile(position);
                    if (tile == null) continue;
                    anyWorldTile = true;
                    if (validTileNames.Contains(tile.name))
                    {
                        validPositions.Add(position);
                        break;
                    }
                }
            }

            if (!anyWorldTile && allowNoTile)
                validPositions.Add(position);
        }

        _logger.Verbose($"Valid tiles in area: {validPositions.Count}");
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

        _logger.Verbose($"Number of spawn positions after proximity filtering: {filteredPositions.Count}");
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
        if (SpawnConfigs == null || SpawnConfigs.Count <= 1)
            return;

        foreach (var spawnConfig in SpawnConfigs)
        {
            if (spawnConfig.PrefabVariants == null || spawnConfig.PrefabVariants.Count == 0)
                continue;

            OccupyWorldObjectSpace initialSpaceOccupied = spawnConfig.PrefabVariants[0].GetComponent<OccupyWorldObjectSpace>();

            foreach (var variant in spawnConfig.PrefabVariants)
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
