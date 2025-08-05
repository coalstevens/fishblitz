using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class BirdSpawner : MonoBehaviour
{
    [SerializeField] private List<BirdSeasonSpawnData> _birdSpawnCalendar; // 0 = Spring, 1 = Summer, 2 = Fall, 3 = Winter
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

    private void SpawnBirds()
    {
        int weekIndex = (GameClock.Instance.GameDay - 1) / 5;
        int seasonIndex = (int)GameClock.Instance.GameSeason;
        var weekData = _birdSpawnCalendar[seasonIndex].GetWeekData(weekIndex);
        var savedBirds = _birdSaveManager.LoadBirds();
        bool retainSpawnPositions = GameClock.Instance.GameMinutesElapsed - _birdSaveManager.LastSpawnTime < RespawnAfterGameMinutesAway;

        Assert.IsNotNull(weekData, $"Week data for season {GameClock.Instance.GameSeason} week {weekIndex + 1} is null. Please check your BirdSpawnCalendar setup.");

        if (!retainSpawnPositions)
        {
            _birdSaveManager.LastSpawnTime = GameClock.Instance.GameMinutesElapsed;
        }

        if (IsSavedBirdDataValid(savedBirds, weekData))
        {
            foreach (var saveData in savedBirds)
            {
                Vector2 spawnPoint = retainSpawnPositions ? new Vector2(saveData.xSpawnPosition, saveData.ySpawnPosition) : GetPointWithinWorldAndOutsideCamera();
                SpawnBird(FindSpeciesByName(saveData.SpeciesName), spawnPoint, saveData);
            }
        }
        else 
        {
            foreach (var spawnData in weekData.DailySpawns)
            {
                for (int i = 0; i < spawnData.SpawnCount; i++)
                {
                    Vector2 spawnPoint = GetPointWithinWorldAndOutsideCamera();
                    SpawnBird(spawnData.SpeciesData, spawnPoint, null);
                }
            }
        }
    }

    private bool IsSavedBirdDataValid(List<BirdSceneSaveManager.BirdSaveData> savedBirds, BirdSeasonSpawnData.WeekData weekData)
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

    private void SpawnBird(BirdSpeciesData birdSpecies, Vector2 spawnPoint, BirdSceneSaveManager.BirdSaveData saveData)
    {
        Assert.IsNotNull(_birdPrefab, $"Bird prefab is not set in bird species: {birdSpecies.name}");
        GameObject spawnedBird = Instantiate
        (
            _birdPrefab,
            spawnPoint,
            Quaternion.identity,
            transform
        );

        BirdBrain _brain = spawnedBird.GetComponent<BirdBrain>();
        _brain.SpeciesData = birdSpecies;
        _brain.InstanceData = new BirdBrain.BirdInstanceData();
        _brain.InstanceData.SeasonSpawned = GameClock.Instance.GameSeason;
        _brain.InstanceData.PeriodSpawned = GameClock.Instance.GameDayPeriod;

        BirdAnimatorController _animator = spawnedBird.GetComponent<BirdAnimatorController>();
        _animator.Initialize();

        if (saveData != null)
        {
            _brain.InstanceData.IsTagged.Value = saveData.IsTagged;
        }

    }

    private Vector2 GetPointWithinWorldAndOutsideCamera()
    {
        Bounds _cameraBounds = GetCameraFrameBounds();
        Vector2 _randomPoint;

        while (true)
        {
            _randomPoint = new Vector2(
                Random.Range(_worldBounds.min.x, _worldBounds.max.x),
                Random.Range(_worldBounds.min.y, _worldBounds.max.y)
            );
            if (!_cameraBounds.Contains(_randomPoint)) break;
        }

        return _randomPoint;
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
