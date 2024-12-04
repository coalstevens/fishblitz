using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

[Serializable]
public class FlyingState : IBirdState
{
    [SerializeField] private Vector2 _flyingDurationRange = new Vector2(2f, 20f);
    [SerializeField] private float _forceUpdateIntervalSecs = 0.5f;
    [SerializeField] private float _wanderRingRadius = 2f;
    [SerializeField] private float _wanderRingDistance = 2f;

    [Header("Flocking Behavior")]
    [SerializeField] private int _boidsMaxFlockSize = 5;
    [SerializeField] private float _separationWeight = 1.0f;
    [SerializeField] private float _alignmentWeight = 1.0f;
    [SerializeField] private float _cohesionWeight = 1.0f;

    [Header("Avoidance Behavior")]
    [SerializeField] private float _avoidanceWeight = 1.0f;
    [SerializeField] private float _circleCastRadius = 1.0f;
    [SerializeField] private float _circleCastRangeRadius = 3.0f;

    private float _lastBoidForceUpdateTime = 0;
    [SerializeField] private Vector2 _boidForce = Vector2.zero;
    [SerializeField] private Vector2 _wanderForce = Vector2.zero;
    [SerializeField] private Vector2 _avoidanceForce = Vector2.zero;
    

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
        if (bird.TickAndCheckBehaviorTimer())
        {
            bird.TransitionToState(bird.Landing);
            return;
        }

        if (Time.time - _lastBoidForceUpdateTime >= _forceUpdateIntervalSecs)
        {
            UpdateBoidForce(bird);
            _lastBoidForceUpdateTime = Time.time;
        }

        UpdateWanderForce(bird);
        UpdateAvoidanceForce(bird);
        bird.RigidBody.AddForce(_wanderForce + _boidForce + _avoidanceForce);
        bird.RigidBody.velocity = Vector2.ClampMagnitude(bird.RigidBody.velocity, bird.FlightSpeedLimit);
    }

    private void UpdateWanderForce(BirdBrain bird) {
        Vector2 _ringCenter = (Vector2)bird.transform.position + bird.RigidBody.velocity.normalized * _wanderRingDistance;
        bird.TargetPosition = _ringCenter + _wanderRingRadius * UnityEngine.Random.insideUnitCircle.normalized;
        _wanderForce = bird.Seek(bird.TargetPosition);
    }

    private void UpdateBoidForce(BirdBrain bird)
    {
        _lastBoidForceUpdateTime = Time.time;

        if (bird.FlockableBirdsNames.Count > 0)
            _boidForce = CalculateBoidForce(bird);
        else 
            _boidForce = Vector2.zero;
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
            return
                (_separation.normalized * _separationWeight +
                _alignment.normalized * _alignmentWeight +
                _cohesion * _cohesionWeight).normalized;
        }
        return Vector2.zero;
    }

    private void UpdateAvoidanceForce(BirdBrain bird) {
        RaycastHit2D hit = Physics2D.CircleCast(
            bird.transform.position, 
            _circleCastRadius, 
            bird.RigidBody.velocity.normalized, 
            _circleCastRangeRadius
        );
        
        if (hit.collider != null)
        {
            Vector2 obstaclePosition = hit.point;
            Vector2 avoidanceDirection = ((Vector2) bird.transform.position - obstaclePosition).normalized;
            float proximityFactor = 1 - (hit.distance / _circleCastRangeRadius); // Closer -> stronger
            _avoidanceForce = avoidanceDirection * proximityFactor * _avoidanceWeight;
        }
        else
        _avoidanceForce = Vector2.zero;
    }
}