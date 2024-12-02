using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class LandingState : IBirdState
{
    [Range(0f, 1f)][SerializeField] private float _perchPreference = 0.33f;
    [Range(0f, 1f)][SerializeField] private float _shelterPreference = 0.33f;
    [Range(0f, 1f)][SerializeField] private float _groundPreference = 0.34f;
    [SerializeField] private float _targetReachedDistanceThreshold = 0.05f;
    private IBirdState _landingSpotState;

    public void Enter(BirdBrain bird)
    {
        SelectPreferredLandingSpot(bird); // Fly to a shelter, perch, ground, etc
    }

    public void Exit(BirdBrain bird)
    {
        bird.RigidBody.velocity = Vector2.zero;
    }

    public void Update(BirdBrain bird)
    {
        if (Vector2.Distance(bird.TargetPosition, bird.transform.position) > _targetReachedDistanceThreshold)
        {
            bird.RigidBody.AddForce(bird.Seek(bird.TargetPosition));
            return;
        }
        bird.TransitionToState(_landingSpotState);
    }

    private void SelectPreferredLandingSpot(BirdBrain bird)
    {
        float _randomValue = UnityEngine.Random.Range(0f, _perchPreference + _shelterPreference + _groundPreference);
        if (_randomValue < _perchPreference)
        {
            if (TrySelectLandingSpotWithACollider<IPerchable>(bird))
            {
                _landingSpotState = bird.PerchedState;
                return;
            }
        }
        else if (_randomValue < _perchPreference + _shelterPreference)
        {
            if (TrySelectLandingSpotWithACollider<IShelterable>(bird))
            {
                _landingSpotState = bird.ShelteredState;
                return;
            }
        }
        else if (_randomValue <= _perchPreference + _shelterPreference + _groundPreference)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!IsLandingSpotWater(bird.TargetPosition))
                {
                    _landingSpotState = bird.GroundedState;
                    return;
                }
                bird.TargetPosition = GetRandomPointInView(bird);
            }
        }

        bird.TransitionToState(bird.FlyingState); // default to flying
    }

    private Vector2 GetRandomPointInView(BirdBrain bird)
    {
        CircleCollider2D collider = (CircleCollider2D)bird.ViewDistance;
        Vector2 center = collider.transform.position + (Vector3)collider.offset;
        Vector2 randomPointInUnitCircle = UnityEngine.Random.insideUnitCircle;
        float radius = collider.radius * Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.y);

        return center + randomPointInUnitCircle * radius;
    }

    private bool IsLandingSpotWater(Vector2 position)
    {
        Tilemap[] _tilemaps = UnityEngine.Object.FindObjectsOfType<Tilemap>();
        foreach (Tilemap _tilemap in _tilemaps)
        {
            if (IsWorldPositionInTilemap(_tilemap, position))
            {
                string _layerName = LayerMask.LayerToName(_tilemap.gameObject.layer);
                if (_layerName == "Water")
                    return true;
            }
        }
        return false;
    }

    private bool IsWorldPositionInTilemap(Tilemap tilemap, Vector3 worldPosition)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        return tilemap.GetTile(cellPosition) != null;
    }

    private bool TrySelectLandingSpotWithACollider<T>(BirdBrain bird) where T : IBirdLandingSpot
    {
        List<Collider2D> _collidersInViewRange = new();
        List<T> _availableSpots = new();
        bird.ViewDistance.OverlapCollider(new ContactFilter2D().NoFilter(), _collidersInViewRange);

        foreach (var _collider in _collidersInViewRange)
            if (_collider.TryGetComponent<T>(out var spot) && (spot is not IPerchable perchable || perchable.IsThereSpace()))
                _availableSpots.Add(spot);

        if (_availableSpots.Count == 0)
            return false;

        bird.TargetBirdSpot = _availableSpots[UnityEngine.Random.Range(0, _availableSpots.Count)];
        bird.TargetPosition = bird.TargetBirdSpot.GetPositionTarget();

        return true;
    }
}