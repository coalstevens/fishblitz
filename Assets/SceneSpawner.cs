using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

// TODO?: This class isn't as efficient as it could be, but its not run frequently so it's probably not an issue
// TODO: The spawn density behavior isn't exactly what i want
public class SceneSpawner : MonoBehaviour
{
    [Serializable]
    private class SpawnObjectInfo
    {
        public List<GameObject> PrefabVariants; // All variants MUST have the same world space size
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

                // Apply filters
                _validSpawnPositions = RemovePositionsThatAreOccupied(_validSpawnPositions);
                _validSpawnPositions = RemovePositionsThatAreTooSmall(_validSpawnPositions, _spaceOccupiedByObject);
                if (_spawnObject.UsePerlinNoise)
                    _validSpawnPositions = ApplyPerlinNoiseToPositions(_validSpawnPositions, _area, _spawnObject);
                if (_spawnObject.SpawnNearbyOtherTypes)
                    _validSpawnPositions = RemovePositionsNotNearTypes(_validSpawnPositions, _spawnObject.NearbyTypes, _area, _spawnObject.NearbyAdjacentDistance);

                // Spawn objects
                _validSpawnPositions = Shuffle(_validSpawnPositions);
                while (_numToSpawn > 0 && _validSpawnPositions.Count > 0)
                {
                    Vector3Int _tilePos = _validSpawnPositions[0];
                    Vector3 _worldPos = _area.CellToWorld(_tilePos) + new Vector3(0.5f, 0, 0);
                    
                    var _variant = _spawnObject.PrefabVariants[UnityEngine.Random.Range(0, _spawnObject.PrefabVariants.Count)];
                    GameObject _spawnedObj = Instantiate(_variant, _worldPos, Quaternion.identity, _impermanentContainer);
                    
                    if (!_spawnedObjects.ContainsKey(_variant))
                        _spawnedObjects[_variant] = new List<Vector3Int>();
                    
                    _spawnedObjects[_variant].Add(_tilePos);

                    _validSpawnPositions = RemovePositionsOccupiedByNewObject(_validSpawnPositions, _tilePos, _spaceOccupiedByObject);
                    _numToSpawn--;
                }
            }
        }
    }

    private List<Vector3Int> ApplyPerlinNoiseToPositions(List<Vector3Int> positions, Tilemap area, SpawnObjectInfo spawnObject)
    {
        List<Vector3Int> _sortedPositions = new List<Vector3Int>();
        
        foreach (var _pos in positions)
        {
            float _perlinValue = Mathf.PerlinNoise(_pos.x * spawnObject.PerlinNoiseScale, _pos.y * spawnObject.PerlinNoiseScale);
            
            if (_perlinValue > spawnObject.PerlinThreshold)  // higher perlin threshold indicates more likelihood of spawning
                _sortedPositions.Add(_pos);
        }

        _logger.Info($"Valid positions after Perlin noise filtering: {_sortedPositions.Count}");
        return _sortedPositions;
    }

    private List<Vector3Int> RemovePositionsOccupiedByNewObject(List<Vector3Int> validSpawnPositions, Vector3Int tilePos, OccupyWorldObjectSpace spaceOccupiedByObject)
    {
        if (spaceOccupiedByObject == null)
        {
            Debug.LogWarning("Object does not have an OccupyWorldObjectSpace component.");
            return validSpawnPositions;
        }

        HashSet<Vector3Int> _occupiedPositions = new HashSet<Vector3Int>();

        for (int x = -spaceOccupiedByObject.extraTilesToLeft; x <= spaceOccupiedByObject.extraTilesToRight; x++)
            for (int y = -spaceOccupiedByObject.extraTilesBelow; y <= spaceOccupiedByObject.extraTilesAbove; y++)
                _occupiedPositions.Add(new Vector3Int(tilePos.x + x, tilePos.y + y, tilePos.z));

        validSpawnPositions.RemoveAll(pos => _occupiedPositions.Contains(pos));
        return validSpawnPositions;
    }

    private List<Vector3Int> RemovePositionsThatAreOccupied(List<Vector3Int> validSpawnPositions)
    {
        if (_occupancyMap == null)
        {
            Debug.LogError("Occupancy map is missing!");
            return validSpawnPositions;
        }

        List<Vector3Int> _filteredTiles = new List<Vector3Int>();

        foreach (var _pos in validSpawnPositions)
            if (!_occupancyMap.CheckOccupied(_pos)) 
                _filteredTiles.Add(_pos);

        _logger.Info($"Number of spawn positions after removing occupied spaces: {_filteredTiles.Count}");
        return _filteredTiles;
    }

    private List<Vector3Int> RemovePositionsThatAreTooSmall(List<Vector3Int> spawnPositions, OccupyWorldObjectSpace spaceOccupiedByObject)
    {
        if (spaceOccupiedByObject == null)
        {
            Debug.LogWarning("Object does not have an OccupyWorldObjectSpace component.");
            return spawnPositions;
        }
        List<Vector3Int> _filteredTiles = new List<Vector3Int>();

        foreach (var _pos in spawnPositions)
        {
            bool _hasSpace = true;

            for (int x = -spaceOccupiedByObject.extraTilesToLeft; x <= spaceOccupiedByObject.extraTilesToRight; x++)
            {
                for (int y = -spaceOccupiedByObject.extraTilesBelow; y <= spaceOccupiedByObject.extraTilesAbove; y++)
                {
                    Vector3Int _checkPos = new Vector3Int(_pos.x + x, _pos.y + y, _pos.z);
                    if (!spawnPositions.Contains(_checkPos))
                    {
                        _hasSpace = false;
                        break;
                    }
                }
                if (!_hasSpace) break;
            }

            if (_hasSpace)
                _filteredTiles.Add(_pos);
        }
        _logger.Info($"Number of spawn positions after filtering by object size: {_filteredTiles.Count}");
        return _filteredTiles;
    }

    private List<Vector3Int> GetPositionsOfValidTilesInArea(Tilemap targetTilemap, List<TileBase> validTiles)
    {
        List<Vector3Int> _validPositions = new List<Vector3Int>();
        BoundsInt _bounds = targetTilemap.cellBounds;

        foreach (Vector3Int _position in _bounds.allPositionsWithin)
        {
            if (!targetTilemap.HasTile(_position)) 
                continue; 
            
            bool _isValid = _allTilemaps.Any(tilemap => validTiles.Contains(tilemap.GetTile(_position)));

            if (_isValid)
                _validPositions.Add(_position);
        }

        _logger.Info($"Valid tiles in area: {_validPositions.Count}");
        return _validPositions;
    }

    private List<Vector3Int> RemovePositionsNotNearTypes(List<Vector3Int> spawnPositions, List<GameObject> nearTypes, Tilemap tilemap, int adjacentDistance)
    {
        if (nearTypes == null || nearTypes.Count == 0)
            return spawnPositions;

        List<Vector3Int> _filteredPositions = new List<Vector3Int>();
        GameObject[] _existingObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (var _pos in spawnPositions)
        {
            bool _foundNearby = false;

            for (int x = -adjacentDistance; x <= adjacentDistance; x++)
            {
                for (int y = -adjacentDistance; y <= adjacentDistance; y++)
                {
                    Vector3Int _checkPos = new Vector3Int(_pos.x + x, _pos.y + y, _pos.z);

                    // Check spawned objects
                    foreach (var _nearType in nearTypes)
                    {
                        if (_spawnedObjects.TryGetValue(_nearType, out List<Vector3Int> spawnedPositions) && spawnedPositions.Contains(_checkPos))
                        {
                            _foundNearby = true;
                            break;
                        }
                    }

                    // Check all GameObjects in the scene
                    foreach (var _obj in _existingObjects)
                    {
                        if (nearTypes.Contains(_obj))
                        {
                            Vector3Int objTilePos = tilemap.WorldToCell(_obj.transform.position);
                            if (objTilePos == _checkPos)
                            {
                                _foundNearby = true;
                                break;
                            }
                        }
                    }
                    if (_foundNearby) break;
                }
                if (_foundNearby) break;
            }

            if (_foundNearby)
                _filteredPositions.Add(_pos);
        }

        _logger.Info($"Number of spawn positions after proximity filtering: {_filteredPositions.Count}");
        return _filteredPositions;
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

        foreach (var _spawnObject in ObjectsToSpawn)
        {
            if (_spawnObject.PrefabVariants == null || _spawnObject.PrefabVariants.Count == 0)
                continue;

            OccupyWorldObjectSpace _initialSpaceOccupied = _spawnObject.PrefabVariants[0].GetComponent<OccupyWorldObjectSpace>();

            foreach (var _variant in _spawnObject.PrefabVariants)
            {
                OccupyWorldObjectSpace spaceOccupied = _variant.GetComponent<OccupyWorldObjectSpace>();

                if (spaceOccupied == null)
                {
                    Debug.LogError($"Prefab {_variant.name} does not have an OccupyWorldObjectSpace component.");
                    continue;
                }

                if (_initialSpaceOccupied.extraTilesToLeft != spaceOccupied.extraTilesToLeft ||
                    _initialSpaceOccupied.extraTilesToRight != spaceOccupied.extraTilesToRight ||
                    _initialSpaceOccupied.extraTilesBelow != spaceOccupied.extraTilesBelow ||
                    _initialSpaceOccupied.extraTilesAbove != spaceOccupied.extraTilesAbove)
                {
                    Debug.LogError($"Prefab variants for {_variant.name} do not occupy the same space.");
                    return;
                }
            }
        }
    }
}