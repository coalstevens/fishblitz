using System.Collections.Generic;
using System.Linq;
using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BirdSpawner : MonoBehaviour
{
    [Serializable]
    private class SpeciesSpawnData
    {
        public BirdSpeciesData SpeciesData;
        public int SpawnCount;
        public List<Tilemap> SpawnAreas = new();
    }

    [Serializable]
    private class WeekData
    {
        [Tooltip("Birds that spawn this week.")]
        public List<SpeciesSpawnData> DailySpawns = new();
    }

    [Serializable]
    private class BirdSeasonSpawnData
    {
        [SerializeField] public WeekData[] springData = new WeekData[3];
        [SerializeField] public WeekData[] summerData = new WeekData[3];
        [SerializeField] public WeekData[] fallData = new WeekData[3];
        [SerializeField] public WeekData[] winterData = new WeekData[3];
    }

    [SerializeField] private BirdSeasonSpawnData _allSpawnData;
    [SerializeField] private BirdSceneSaveManager _birdSaveManager;
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private string _birdSpeciesResourcePath = "Birds"; // Path under Resources
    [SerializeField] Collider2D _world;
    [SerializeField] private int RespawnAfterGameMinutesAway = 60;
    [SerializeField] private GameObject _birdPrefab;

    private Bounds _worldBounds;
    private Camera _mainCamera;
    private List<BirdSpeciesData> allSpecies;

    private void OnEnable()
    {
        allSpecies = Resources.LoadAll<BirdSpeciesData>(_birdSpeciesResourcePath).ToList();
        Assert.IsNotNull(allSpecies, "All species was not found");
        Assert.IsTrue(allSpecies.Count > 0, "All species is empty");
        Assert.IsNotNull(_world, "World collider not assigned.");
        Assert.IsNotNull(_playerData, "Player data not assigned.");
        Assert.IsNotNull(_birdSaveManager, "Bird save manager not assigned");

        _worldBounds = _world.bounds;
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        bool isNewWeek = (GameClock.Instance.GameDay - 1) % 5 == 0;
        if (isNewWeek)
        {
            _birdSaveManager.ClearSceneSaveData();
        }

        SpawnBirds();
    }

    private void OnDisable()
    {
        _birdSaveManager.SaveBirds(GetComponentsInChildren<BirdBrain>().ToList());

        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    private WeekData[] GetSeasonData(GameClock.Seasons season)
    {
        switch (season)
        {
            case GameClock.Seasons.Spring:
                return _allSpawnData.springData;
            case GameClock.Seasons.Summer:
                return _allSpawnData.summerData;
            case GameClock.Seasons.Fall:
                return _allSpawnData.fallData;
            case GameClock.Seasons.Winter:
                return _allSpawnData.winterData;
            default:
                Debug.LogError("Unexpected code path");
                return null;
        }
    }

    private void SpawnBirds()
    {
        int currentWeekIndex = (GameClock.Instance.GameDay - 1) / 5;
        WeekData[] seasonData = GetSeasonData(GameClock.Instance.GameSeason);
        var weekData = seasonData[currentWeekIndex];
        var savedBirds = _birdSaveManager.LoadBirds();
        bool retainSpawnPositions = GameClock.Instance.GameMinutesElapsed - _birdSaveManager.LastSpawnTime < RespawnAfterGameMinutesAway;

        Assert.IsNotNull(weekData, $"Week data for season {GameClock.Instance.GameSeason} week {currentWeekIndex + 1} is null. Please check the BirdSpawnCalendar setup.");

        if (!retainSpawnPositions)
        {
            _birdSaveManager.LastSpawnTime = GameClock.Instance.GameMinutesElapsed;
        }

        // Spawn using saved positions
        if (IsSavedBirdDataValid(savedBirds, weekData) && retainSpawnPositions)
        {
            foreach (var saveData in savedBirds)
            {
                var matchingSpawnData = weekData.DailySpawns
                    .FirstOrDefault(s => s.SpeciesData.name == saveData.SpeciesName);

                if (matchingSpawnData == null)
                {
                    Debug.LogWarning($"No matching spawn data found for species {saveData.SpeciesName}");
                    continue;
                }

                Vector2 spawnPoint = new Vector2(saveData.xSpawnPosition, saveData.ySpawnPosition);
                SpawnBird(matchingSpawnData, spawnPoint, saveData);
            }
        }
        // Spawn new birds
        else
        {
            foreach (var spawnData in weekData.DailySpawns)
            {
                for (int i = 0; i < spawnData.SpawnCount; i++)
                {
                    Vector2 spawnPoint = GetPointInTilemapsAndOutsideCamera(spawnData.SpawnAreas);
                    SpawnBird(spawnData, spawnPoint, null);
                }
            }
        }
    }

    private bool IsSavedBirdDataValid(List<BirdSceneSaveManager.BirdSaveData> savedBirds, WeekData weekData)
    {
        int expectedTotal = weekData.DailySpawns.Sum(s => s.SpawnCount);
        if (savedBirds.Count != expectedTotal)
            return false;

        var savedFreq = savedBirds
            .GroupBy(s => s.SpeciesName)
            .ToDictionary(g => g.Key, g => g.Count());

        var expectedFreq = weekData.DailySpawns
            .GroupBy(s => s.SpeciesData.name)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.SpawnCount));

        foreach (var (species, expectedCount) in expectedFreq)
        {
            if (!savedFreq.TryGetValue(species, out var actualCount) || actualCount != expectedCount)
                return false;
        }

        foreach (var species in savedFreq.Keys)
        {
            if (!expectedFreq.ContainsKey(species))
                return false;
        }

        return true;
    }

    private void SpawnBird(SpeciesSpawnData spawnData, Vector2 spawnPoint, BirdSceneSaveManager.BirdSaveData saveData)
    {
        Assert.IsNotNull(_birdPrefab, $"Bird prefab is not set in bird species: {spawnData.SpeciesData.name}");
        GameObject spawnedBird = Instantiate
        (
            _birdPrefab,
            spawnPoint,
            Quaternion.identity,
            transform
        );

        BirdBrain _brain = spawnedBird.GetComponent<BirdBrain>();
        _brain.SpeciesData = spawnData.SpeciesData;
        _brain.InstanceData = new BirdBrain.BirdInstanceData
        {
            SeasonSpawned = GameClock.Instance.GameSeason,
            PeriodSpawned = GameClock.Instance.GameDayPeriod
        };
        _brain.GetNewSpawnPoint = () => GetPointInTilemapsAndOutsideCamera(spawnData.SpawnAreas);
        _brain.SetTagListener();
        BirdAnimatorController _animator = spawnedBird.GetComponent<BirdAnimatorController>();
        _animator.Initialize();

        if (saveData != null)
        {
            _brain.InstanceData.IsTagged.Value = saveData.IsTagged;
        }

    }

    private Vector2 GetPointInTilemapsAndOutsideCamera(List<Tilemap> spawnTilemaps)
    {
        var shuffledTilemaps = spawnTilemaps
            .Where(tm => tm != null)
            .OrderBy(_ => UnityEngine.Random.value)
            .ToList();

        Bounds cameraBounds = GetCameraFrameBounds();

        foreach (var map in shuffledTilemaps)
        {
            var tilePositions = new List<Vector2>();
            foreach (var pos in map.cellBounds.allPositionsWithin)
            {
                if (map.HasTile(pos))
                {
                    Vector2 worldPos = map.CellToWorld(pos) + map.tileAnchor;
                    tilePositions.Add(worldPos);
                }
            }

            if (tilePositions.Count == 0) continue;

            tilePositions = tilePositions.OrderBy(_ => UnityEngine.Random.value).ToList();

            foreach (var position in tilePositions)
                if (!cameraBounds.Contains(position))
                    return position;
        }

        Debug.LogWarning("Failed to find spawn point outside of camera in any tilemap.");
        return Vector2.zero;
    }

    private Bounds GetCameraFrameBounds()
    {
        float _cameraZPosition = Mathf.Abs(_mainCamera.transform.position.z);

        Vector3 _bottomLeft = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, _cameraZPosition));
        Vector3 _topRight = _mainCamera.ViewportToWorldPoint(new Vector3(1, 1, _cameraZPosition));

        Bounds _cameraBounds = new Bounds();
        _cameraBounds.SetMinMax(_bottomLeft, _topRight);

        return _cameraBounds;
    }

    private BirdSpeciesData FindSpeciesByName(string name)
    {
        foreach (var species in allSpecies)
        {
            if (species.name == name)
                return species;
        }
        Debug.LogError($"BirdSpecies with name '{name}' not found in Resources/{_birdSpeciesResourcePath}");
        return null;
    }
}
