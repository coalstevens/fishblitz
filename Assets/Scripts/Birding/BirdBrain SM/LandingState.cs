using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class LandingState : IBirdState
{
    [Range(0f, 1f)][SerializeField] private float _perchPreference = 0.33f;
    [Range(0f, 1f)][SerializeField] private float _shelterPreference = 0.33f;
    [Range(0f, 1f)][SerializeField] private float _groundPreference = 0.34f;
    [SerializeField] private float _targetProximityThreshold = 0.05f;
    [SerializeField] private float _landingTimeoutTeleport = 5f;
    [SerializeField] public float FlockLandingCircleRadius = 2f;
    private float _landingStartTime;
    private IBirdState _stateOnTargetReached;

    // The area in which the bird searches for a landing spot
    private Vector2 _landingCircleCenter;
    private float _landingCircleRadius;
    private bool _landingCirclePreset = false;

    public void Enter(BirdBrain bird)
    {
        UpdateLandingCircle(bird);
        SelectPreferredLandingSpotInLandingCircle(bird); // Fly to a shelter, perch, ground, etc
        ToggleCollisionsForLandingSpot(bird);
        _landingStartTime = Time.time;
    }

    public void Exit(BirdBrain bird)
    {
        bird.RigidBody.velocity = Vector2.zero;
    }

    public void Update(BirdBrain bird)
    {
        // Teleport if landing is taking too long
        if (Time.time - _landingStartTime >= _landingTimeoutTeleport)
        {
            bird.transform.position = bird.TargetPosition;
        }

        // Add force while far from target 
        else if (Vector2.Distance(bird.TargetPosition, bird.transform.position) > _targetProximityThreshold)
        {
            bird.RigidBody.AddForce(bird.Seek(bird.TargetPosition));
            return;
        }

        bird.TransitionToState(_stateOnTargetReached);
    }

    private void UpdateLandingCircle(BirdBrain bird)
    {
        // Landing circle set externally
        if (_landingCirclePreset) { 
            _landingCirclePreset = false;
            return;
        }
        
        // Default is to use ViewDistanceCollider
        if (bird.ViewDistance is CircleCollider2D circle)
            SetLandingCircle(circle.radius, (Vector2) circle.transform.position + circle.offset);
        else
            Debug.LogError("ViewDistance on bird must be a circle collider");
    }

    public void SetLandingCircle(float radius, Vector2 center)
    {
        _landingCirclePreset = true;
        _landingCircleRadius = radius;
        _landingCircleCenter = center;
    }

    private void SelectPreferredLandingSpotInLandingCircle(BirdBrain bird)
    {
        float _randomValue = UnityEngine.Random.Range(0f, _perchPreference + _shelterPreference + _groundPreference);
        if (_randomValue < _perchPreference)
        {
            if (TryFindLandingSpotOfType<IPerchable>(bird))
            {
                _stateOnTargetReached = bird.Perched;
                return;
            }
        }
        else if (_randomValue < _perchPreference + _shelterPreference)
        {
            if (TryFindLandingSpotOfType<IShelterable>(bird))
            {
                _stateOnTargetReached = bird.Sheltered;
                return;
            }
        }
        else if (_randomValue <= _perchPreference + _shelterPreference + _groundPreference)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!IsTargetOverWater(bird.TargetPosition))
                {
                    _stateOnTargetReached = bird.Grounded;
                    return;
                }
                bird.TargetPosition = GeneratePointInLandingCircle(bird);
            }
        }

        bird.TransitionToState(bird.Flying); // default to flying
    }

    private void ToggleCollisionsForLandingSpot(BirdBrain bird)
    {
        if (_stateOnTargetReached == bird.Sheltered || _stateOnTargetReached == bird.Perched)
            bird.BirdCollider.isTrigger = true;
    }

    private Vector2 GeneratePointInLandingCircle(BirdBrain bird)
    {
        return _landingCircleCenter + UnityEngine.Random.insideUnitCircle * _landingCircleRadius;
    }

    private bool IsTargetOverWater(Vector2 birdPosition)
    {
        Tilemap[] _tilemaps = UnityEngine.Object.FindObjectsOfType<Tilemap>();
        foreach (Tilemap _tilemap in _tilemaps)
        {
            if (IsPositionWithinTilemap(_tilemap, birdPosition))
            {
                string _layerName = LayerMask.LayerToName(_tilemap.gameObject.layer);
                if (_layerName == "Water")
                    return true;
            }
        }
        return false;
    }

    private bool IsPositionWithinTilemap(Tilemap tilemap, Vector2 worldPosition)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        return tilemap.GetTile(cellPosition) != null;
    }

    private bool TryFindLandingSpotOfType<T>(BirdBrain bird) where T : IBirdLandingSpot
    {
        List<Collider2D> _collidersInLandingCircle = Physics2D.OverlapCircleAll(_landingCircleCenter, _landingCircleRadius).ToList();
        List<T> _availableSpots = new();
        
        foreach (var _collider in _collidersInLandingCircle)
            if (_collider.TryGetComponent<T>(out var spot) && (spot is IShelterable || (spot is IPerchable perchable && perchable.IsThereSpace())))
                _availableSpots.Add(spot);

        if (_availableSpots.Count == 0)
            return false;

        bird.LandingTargetSpot = _availableSpots[UnityEngine.Random.Range(0, _availableSpots.Count)];
        bird.TargetPosition = bird.LandingTargetSpot.GetPositionTarget();
        if (bird.LandingTargetSpot is IPerchable perch)
            perch.ReserveSpace(bird);

        return true;
    }
}