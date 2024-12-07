using System;
using UnityEngine;

[Serializable]
public class FlyingState : IBirdState
{
    [SerializeField] private Vector2 _flyingDurationRange = new Vector2(2f, 20f);
    [SerializeField] private float _speedLimit = 2;
    [Range(0f, 1f)] [SerializeField] private float _landingPreference = 0.75f;
    [Range(0f, 1f)] [SerializeField] private float _soaringPreference = 0.25f;

    [Header("Wander Force")]
    [SerializeField] private float _wanderRingRadius = 2f;
    [SerializeField] private float _wanderRingDistance = 2f;
    [SerializeField] private float _steerForceLimit = 4f;

    [Header("Flocking Force")]
    [SerializeField] private float _boidForceUpdateIntervalSecs = 0.5f; // To improve performance
    [SerializeField] private int _maxFlockMates = 5;
    [SerializeField] private float _separationWeight = 1.0f;
    [SerializeField] private float _alignmentWeight = 1.0f;
    [SerializeField] private float _cohesionWeight = 1.0f;

    [Header("Avoidance Force")]
    [SerializeField] private float _avoidanceWeight = 1.0f;
    [SerializeField] private float _circleCastRadius = 1.0f;
    [SerializeField] private float _circleCastRange = 3.0f;

    [Header("State observation")]
    [SerializeField] private Vector2 _boidForce = Vector2.zero;
    [SerializeField] private Vector2 _wanderForce = Vector2.zero;
    [SerializeField] private Vector2 _avoidanceForce = Vector2.zero; 
    private float _lastBoidForceUpdateTime = 0;

    public void Enter(BirdBrain bird)
    {
        bird.Animator.Play("Flying");
        bird.BehaviorDuration = UnityEngine.Random.Range(_flyingDurationRange.x, _flyingDurationRange.y);

        bird.BirdCollider.isTrigger = false;
        bird.SpriteSorting.enabled = true;
        bird.Renderer.sortingLayerName = "Main";
    }

    public void Exit(BirdBrain bird)
    {
        // do nothing
    }

    public void Update(BirdBrain bird)
    {
        if (bird.TickAndCheckBehaviorTimer())
        {
            float _randomValue = UnityEngine.Random.Range(0, _landingPreference + _soaringPreference);
            if (_randomValue < _landingPreference)
                bird.TransitionToState(bird.Landing);
            else
                bird.TransitionToState(bird.Soaring);
            return;
        }

        if (Time.time - _lastBoidForceUpdateTime >= _boidForceUpdateIntervalSecs)
        {
            _boidForce = BirdForces.CalculateBoidForce(bird, _maxFlockMates, _separationWeight, _alignmentWeight, _cohesionWeight);
            _lastBoidForceUpdateTime = Time.time;
        }

        _wanderForce = BirdForces.CalculateWanderForce(bird, _speedLimit, _steerForceLimit, _wanderRingDistance, _wanderRingRadius);
        _avoidanceForce = BirdForces.CalculateAvoidanceForce(bird, _circleCastRadius, _circleCastRange,_avoidanceWeight);
        bird.RigidBody.AddForce(_wanderForce + _boidForce + _avoidanceForce);
        bird.RigidBody.velocity = Vector2.ClampMagnitude(bird.RigidBody.velocity, _speedLimit);
    }

}