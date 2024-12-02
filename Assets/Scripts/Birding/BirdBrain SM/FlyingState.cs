using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class FlyingState : IBirdState
{
    [SerializeField] private Vector2 _flyingDurationRange = new Vector2(2f, 20f);
    [SerializeField] private float _forceApplicationIntervalSecs = 0.1f;
    [SerializeField] private float _forceUpdateIntervalSecs = 0.5f;
    [SerializeField] private float _wanderRingRadius = 2f;
    [SerializeField] private float _wanderRingDistance = 2f;

    [Header("Flocking Behavior")]
    [SerializeField] private int _boidsMaxFlockSize = 5;
    [SerializeField] private float _separationWeight = 1.0f;
    [SerializeField] private float _alignmentWeight = 1.0f;
    [SerializeField] private float _cohesionWeight = 1.0f;

    private int _flyingFrameCounter = 0;
    private float _lastForceApplicationTime = 0;
    private float _lastForceUpdateTime = 0;
    private Vector2 _flyingForce = Vector2.zero;

    public void Enter(BirdBrain bird)
    {
        bird.Animator.Play("Flying");
        bird.BehaviorDuration = UnityEngine.Random.Range(_flyingDurationRange.x, _flyingDurationRange.y);
    }

    public void Exit(BirdBrain bird)
    {
        // do nothing
    }

    public void Update(BirdBrain bird)
    {
        if (bird.IsBehaviourDurationExpired())
        {
            bird.TransitionToState(bird.LandingState);
            return;
        }

        _flyingFrameCounter++;
        float _now = Time.time;
        if (_now - _lastForceApplicationTime >= _forceApplicationIntervalSecs)
        {
            bird.RigidBody.AddForce(_flyingForce);
            _lastForceApplicationTime = _now;
        }

        if (_now - _lastForceUpdateTime >= _forceUpdateIntervalSecs)
        {
            UpdateFlyingForce(bird);
            _lastForceUpdateTime = _now;
        }
    }

    private void UpdateFlyingForce(BirdBrain bird)
    {
        // update wander ring target
        Vector2 _ringCenter = (Vector2)bird.transform.position + bird.RigidBody.velocity.normalized * _wanderRingDistance;
        bird.TargetPosition = _ringCenter + _wanderRingRadius * UnityEngine.Random.insideUnitCircle.normalized;
        _lastForceUpdateTime = Time.time;

        _flyingForce = bird.Seek(bird.TargetPosition);
        if (bird.FlockableBirdsNames.Count > 0)
            _flyingForce += CalculateBoidForce(bird);
    }

    private Vector2 CalculateBoidForce(BirdBrain bird)
    {
        Vector2 _separation = Vector2.zero; // Prevents birds getting too close
        Vector2 _alignment = Vector2.zero; // Urge to match direction of others
        Vector2 _cohesion = Vector2.zero; // Urge to move towards centroid of flock
        int _count = 0;
        var _nearbyBirds = bird.NearbyBirdTracker.NearbyBirds
            .Where(b => bird.FlockableBirdsNames.Contains(b.Name));

        foreach (var _nearbyBird in _nearbyBirds)
        {
            if (_nearbyBird.gameObject == null) continue;
            float _distance = Vector2.Distance(bird.transform.position, _nearbyBird.transform.position);
            _separation += (Vector2)(bird.transform.position - _nearbyBird.transform.position) / _distance;
            _alignment += _nearbyBird.GetVelocity();
            _cohesion += (Vector2)_nearbyBird.transform.position;
            _count++;
            if (_count >= _boidsMaxFlockSize)
                break;
        }

        if (_count > 0)
        {
            _separation /= _count;
            _alignment /= _count;
            _cohesion /= _count;
            _cohesion = (_cohesion - (Vector2)bird.transform.position).normalized;
        }

        return
            _separation.normalized * _separationWeight +
            _alignment.normalized * _alignmentWeight +
            _cohesion * _cohesionWeight;
    }
}