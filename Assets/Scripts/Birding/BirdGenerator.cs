using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BirdGenerator : MonoBehaviour
{

    [SerializeField] int _birdsInScene = 15;
    [SerializeField] Collider2D _world;
    [SerializeField] private string _birdDirectioryName = "Birds"; 

    private Bounds _worldBounds;
    private List<GameObject> _allBirds;
    private List<GameObject> _spawnableBirds = new();
    private GameClock.Seasons? _prevSeason = null;
    private GameClock.DayPeriods? _prevPeriod = null;
    private GameClock.Seasons _currSeason;
    private GameClock.DayPeriods _currPeriod;
    private Camera _mainCamera;

    void Start()
    {
        _worldBounds = _world.bounds;
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _allBirds = Resources.LoadAll<GameObject>(_birdDirectioryName).ToList();
        _allBirds = Resources.LoadAll<GameObject>(_birdDirectioryName)?.ToList();
        if (_allBirds == null || _allBirds.Count == 0)
        {
            Debug.LogError("No birds found in the Resources/" + _birdDirectioryName + " folder.");
            return;
        }

        UpdateSpawnableBirdsList();
        if(_spawnableBirds.Count != 0) {
            for (int i = 0; i < _birdsInScene; i++)
                SpawnBird(_spawnableBirds[Random.Range(0, _spawnableBirds.Count)], GetPointWithinWorld()); // birds can spawn in camera view only at the start of the scene
        }
    }

    void Update()
    {
        if (transform.childCount >= _birdsInScene) 
            return;
        
        UpdateSpawnableBirdsList();
        if (_spawnableBirds.Count == 0)
                return;

        SpawnBird(_spawnableBirds[Random.Range(0, _spawnableBirds.Count)], GetPointWithinWorldAndOutsideCamera());
    }

    private void SpawnBird(GameObject bird, Vector2 spawnPoint)
    {
        GameObject _birdInstance = Instantiate
        (
            bird,
            spawnPoint,
            Quaternion.identity,
            transform
        );

        Bird _birdBird = _birdInstance.GetComponent<Bird>();
        if (_birdBird == null)
        {
            Debug.LogError("Spawned object does not have a Bird component.");
            return;
        }
        _birdBird.SeasonSpawned = _currSeason;
        _birdBird.PeriodSpawned = _currPeriod;
    }

    private void UpdateSpawnableBirdsList()
    {
        _currSeason = GameClock.Instance.GameSeason;
        _currPeriod = GameClock.Instance.GetDayPeriod();

        if (_prevSeason == _currSeason && _prevPeriod == _currPeriod)
            return;

        _prevSeason = _currSeason;
        _prevPeriod = _currPeriod;

        _spawnableBirds = _allBirds.Where(b =>
            {
                var birdComponent = b.GetComponent<Bird>();
                return 
                    birdComponent != null &&
                    birdComponent.SpawnableSeasons.Contains(_currSeason) &&
                    birdComponent.SpawnablePeriods.Contains(_currPeriod);
            }).ToList();
    }

    private Vector2 GetPointWithinWorldAndOutsideCamera()
    {
        Bounds _cameraBounds = GetCameraFrameBounds();
        Vector2 _randomPoint;

        while (true)
        {
            _randomPoint = GetPointWithinWorld();
            if (!_cameraBounds.Contains(_randomPoint)) break;
        }

        return _randomPoint;
    }

    private Vector2 GetPointWithinWorld() {
        return new Vector2 (
            Random.Range(_worldBounds.min.x, _worldBounds.max.x),
            Random.Range(_worldBounds.min.y, _worldBounds.max.y)
        );    
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
}
