using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SoaringLandingState : IBirdState
{
    [SerializeField] private float _targetProximityThreshold = 0.05f;
    [SerializeField] private float _landingTimeoutTeleport = 5f;
    [SerializeField] public float FlockLandingCircleRadius = 2f;
    [SerializeField] private float _speedLimit = 3f;
    [SerializeField] private float _steerForceLimit = 4f;
    private float _landingStartTime;
    private IBirdState _stateOnTargetReached;

    // The area in which the bird searches for a landing spot
    private Vector2 _landingCircleCenter;
    private float _landingCircleRadius;
    private bool _wasLandingCirclePreset = false;

    public void Enter(BirdBrain bird)
    {
        bird.Animator.Play("Flying");
        UpdateLandingCircle(bird);
        SelectPreferredLandingSpotInLandingCircle(bird); // Fly to a shelter, perch, ground, etc
        _landingStartTime = Time.time;

        bird.BirdCollider.isTrigger = true;
        bird.SpriteSorting.enabled = false;
        bird.Renderer.sortingLayerName = "Foreground";
    }

    public void Exit(BirdBrain bird)
    {
        bird.RigidBody.velocity = Vector2.zero;
    }

    public void Update(BirdBrain bird)
    {
        if (_stateOnTargetReached == bird.Soaring)
            bird.TransitionToState(_stateOnTargetReached);
            
        // Teleport if landing is taking too long
        if (Time.time - _landingStartTime >= _landingTimeoutTeleport)
        {
            bird.transform.position = bird.TargetPosition;
        }

        // Add force while far from target 
        else if (Vector2.Distance(bird.TargetPosition, bird.transform.position) > _targetProximityThreshold)
        {
            bird.RigidBody.AddForce(BirdForces.Seek(bird, _speedLimit, _steerForceLimit));
            return;
        }

        bird.TransitionToState(_stateOnTargetReached);
    }

    private void UpdateLandingCircle(BirdBrain bird)
    {
        // Landing circle set externally
        if (_wasLandingCirclePreset) { 
            _wasLandingCirclePreset = false;
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
        _wasLandingCirclePreset = true;
        _landingCircleRadius = radius;
        _landingCircleCenter = center;
    }

    private void SelectPreferredLandingSpotInLandingCircle(BirdBrain bird)
    {
        if (bird.TryFindLandingSpotOfType<IPerchableHighElevation>(_landingCircleCenter, _landingCircleRadius))
        {
            _stateOnTargetReached = bird.Perched;
            return;
        }
        _stateOnTargetReached = bird.Soaring;
    }

}