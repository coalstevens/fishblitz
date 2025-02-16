using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneSpawner : MonoBehaviour
{
    [Serializable]
    private class SpawnObjectInfo
    {
        public List<GameObject> PrefabVariants; // All variants MUST have the same world space size
        public List<Tilemap> SpawnAreas = new();
        public List<TileBase> ValidTileTypes = new();
        public float Density = 0.05f;
        public bool SpawnNear = false;
        public float NearRadius = 2f;
        public List<GameObject> NearTypes = new();
        public bool UsePerlinNoise = false;  
        public float PerlinNoiseScale = 0.1f;  
        public float PerlinThreshold = 0.5f;
    }

    [SerializeField] private List<SpawnObjectInfo> ObjectsToSpawn = new();
    [SerializeField] private Logger _logger = new();
    private Transform _impermanentContainer;
    private WorldObjectOccupancyMap _occupancyMap;
    private List<Tilemap> _allTilemaps;

    void Start()
    {
        _impermanentContainer = GameObject.FindGameObjectWithTag("Impermanent")?.transform;
        if (_impermanentContainer == null)
            Debug.LogError("Impermanent container not found! Creating one.");
        _occupancyMap = _impermanentContainer.GetComponent<WorldObjectOccupancyMap>();
        VerifyVariantsAreSameSize();
        SpawnObjects("");
        gameObject.SetActive(false); // Disables all spawn area markers as well (child objects)
    }

    private void OnEnable()
    {
        //SceneSaveLoadManager.FirstVisitToScene += SpawnObjects;
    }

    private void OnDisable()
    {
        //SceneSaveLoadManager.FirstVisitToScene -= SpawnObjects;
    }

    private void SpawnObjects(string sceneName)
    {
        _logger.Info("Spawn start");
        _allTilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None).ToList();
        
        foreach (var _spawnObject in ObjectsToSpawn)
        {
            OccupyWorldObjectSpace _spaceOccupiedByObject = _spawnObject.PrefabVariants[0].GetComponent<OccupyWorldObjectSpace>();
            
            foreach (var _area in _spawnObject.SpawnAreas)
            {
                List<Vector3Int> _validSpawnPositions = GetPositionsOfValidTilesInArea(_area, _spawnObject.ValidTileTypes);
                int _numToSpawn = Mathf.CeilToInt(_validSpawnPositions.Count * _spawnObject.Density);
                _logger.Info($"Attempting to spawn {_numToSpawn} of variants of {_spawnObject.PrefabVariants[0].name}");

                _validSpawnPositions = RemovePositionsThatAreOccupied(_validSpawnPositions);
                _validSpawnPositions = RemovePositionsThatAreTooSmall(_validSpawnPositions, _spaceOccupiedByObject);
                _validSpawnPositions = Shuffle(_validSpawnPositions);

                if (_spawnObject.UsePerlinNoise)
                {
                    _validSpawnPositions = ApplyPerlinNoiseToPositions(_validSpawnPositions, _area, _spawnObject);
                }

                while (_numToSpawn > 0 && _validSpawnPositions.Count > 0)
                {
                    if (_validSpawnPositions.Count == 0) break;

                    Vector3Int tilePos = _validSpawnPositions[0];
                    Vector3 worldPos = _area.CellToWorld(tilePos) + new Vector3(0.5f, 0, 0);

                    _numToSpawn--;
                    var _variant = _spawnObject.PrefabVariants[UnityEngine.Random.Range(0, _spawnObject.PrefabVariants.Count)];
                    Instantiate(_variant, worldPos, Quaternion.identity, _impermanentContainer);
                    _validSpawnPositions = RemovePositionsOccupiedByNewObject(_validSpawnPositions, tilePos, _spaceOccupiedByObject);
                }
            }
        }
    }

    private List<Vector3Int> ApplyPerlinNoiseToPositions(List<Vector3Int> positions, Tilemap area, SpawnObjectInfo spawnObject)
    {
        List<Vector3Int> sortedPositions = new List<Vector3Int>();
        
        foreach (var pos in positions)
        {
            float perlinValue = Mathf.PerlinNoise(pos.x * spawnObject.PerlinNoiseScale, pos.y * spawnObject.PerlinNoiseScale);
            
            // Adjust the spawn probability based on Perlin noise (higher perlin values indicate more likelihood of spawning)
            if (perlinValue > spawnObject.PerlinThreshold)  // Threshold can be adjusted
            {
                sortedPositions.Add(pos);
            }
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
        List<Vector3Int> _filteredTiles = new List<Vector3Int>();

        foreach (var pos in spawnPositions)
        {
            bool hasSpace = true;

            for (int x = -spaceOccupiedByObject.extraTilesToLeft; x <= spaceOccupiedByObject.extraTilesToRight; x++)
            {
                for (int y = -spaceOccupiedByObject.extraTilesBelow; y <= spaceOccupiedByObject.extraTilesAbove; y++)
                {
                    Vector3Int checkPos = new Vector3Int(pos.x + x, pos.y + y, pos.z);
                    if (!spawnPositions.Contains(checkPos))
                    {
                        hasSpace = false;
                        break;
                    }
                }
                if (!hasSpace) break;
            }

            if (hasSpace)
                _filteredTiles.Add(pos);
        }
        _logger.Info($"Number of spawn positions after filtering by object size: {_filteredTiles.Count}");
        return _filteredTiles;
    }

    private List<Vector3Int> GetPositionsOfValidTilesInArea(Tilemap targetTilemap, List<TileBase> validTiles)
    {
        List<Vector3Int> validPositions = new List<Vector3Int>();
        BoundsInt bounds = targetTilemap.cellBounds;

        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (!targetTilemap.HasTile(position)) 
                continue; // Skip if there's no tile in the target tilemap
            
            // Check if any tilemap in the same grid has a valid tile
            bool isValid = _allTilemaps.Any(tilemap => validTiles.Contains(tilemap.GetTile(position)));

            if (isValid)
            {
                validPositions.Add(position);
            }
        }

        _logger.Info($"Valid tiles in area: {validPositions.Count}");
        return validPositions;
    }

    private bool IsNearRequiredType(Vector3 position, List<GameObject> nearTypes, float nearRadius)
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(position, nearRadius);
        foreach (var obj in nearbyObjects)
        {
            if (nearTypes.Contains(obj.gameObject))
                return true;
        }
        return false;
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
                OccupyWorldObjectSpace spaceOccupied = variant.GetComponent<OccupyWorldObjectSpace>();

                if (spaceOccupied == null)
                {
                    Debug.LogError($"Prefab {variant.name} does not have an OccupyWorldObjectSpace component.");
                    continue;
                }

                if (initialSpaceOccupied.extraTilesToLeft != spaceOccupied.extraTilesToLeft ||
                    initialSpaceOccupied.extraTilesToRight != spaceOccupied.extraTilesToRight ||
                    initialSpaceOccupied.extraTilesBelow != spaceOccupied.extraTilesBelow ||
                    initialSpaceOccupied.extraTilesAbove != spaceOccupied.extraTilesAbove)
                {
                    Debug.LogError($"Prefab variants for {variant.name} do not occupy the same space.");
                    return;
                }
            }
        }
    }
}