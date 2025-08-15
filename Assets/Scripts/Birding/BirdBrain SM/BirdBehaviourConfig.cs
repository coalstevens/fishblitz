using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBirdBehaviourConfig", menuName = "Birding/BirdBehaviourConfig")]
public class BirdBehaviourConfig : ScriptableObject
{
    [Header("Flocking")]
    public float ReactionCooldownSecs = 2f; // Bird can only react to a flock mate once every X seconds
    public Vector2 ReactionTimeRangeSecs = new Vector2(0f, 0.5f); // Range of time it takes to react to a flock mate

    [Header("States")]
    public LowFlyingParameters LowFlying = new();
    public LowLandingParameters LowLanding = new();
    public ShelteredParameters Sheltered = new();
    public PerchedParameters Perched = new();
    public GroundedParameters Grounded = new();
    public FleeingParameters Fleeing = new();
    public HighFlyingParameters HighFlying = new();
    public HighLandingParameters HighLanding = new();

    [Serializable]
    public struct LowFlyingParameters
    {
        public Vector2 BehaviourDurationRangeSecs;
        public float SpeedLimit;
        [Range(0f, 1f)] public float LandingPreference;
        [Range(0f, 1f)] public float HighFlyingPreference;

        [Header("Wander Force")]
        public float WanderRingRadius;
        public float WanderRingDistance;
        public float WanderForceUpdateIntervalSecs;
        public float SteerForceLimit;

        [Header("Flocking Force")]
        public float BoidForceWeight;
        public float BoidForceUpdateIntervalSecs; // For performance
        public int MaxFlockMates;
        public float SeparationWeight;
        public float AlignmentWeight;
        public float CohesionWeight;

        [Header("Avoidance Force")]
        public float AvoidanceWeight;
        public float CircleCastRadius;
        public float CircleCastRange;
    }

    [Serializable]
    public struct LowLandingParameters
    {
        [Range(0f, 1f)] public float PerchPreference;
        [Range(0f, 1f)] public float ShelterPreference;
        [Range(0f, 1f)] public float GroundPreference;

        [Header("Landing")]
        public float SpeedLimit;
        public float SnapToTargetDistance;
        public float LandingTimeoutSecs;
        public float StuckMovementThreshold;
        public float FlockLandingCircleRadius;
        public float SteerForceLimit;

        [Header("Avoidance Force")]
        public float AvoidanceWeight;
        public float CircleCastRadius;
        public float CircleCastRange;
    }

    [Serializable]
    public struct ShelteredParameters
    {
        public Vector2 BehaviourDurationRangeSecs;
    }

    [Serializable]
    public struct PerchedParameters
    {
        public Vector2 BehaviourDurationRangeSecs;
    }

    [Serializable]
    public struct GroundedParameters
    {
        public Vector2 BehaviourDurationRangeSecs;
        public Vector2 TimeTillHopRangeSecs;
        public Vector2 TwoHopForceLimits;
    }

    [Serializable]
    public struct FleeingParameters
    {
        public Vector2 BehaviourDurationRangeSecs;
        public float FleeForceMagnitude;
        public float FleeMaxSpeed;

        [Header("Avoidance Force")]
        public float AvoidanceWeight;
        public float CircleCastRadius;
        public float CircleCastRange;
    }
    [Serializable]
    public struct HighFlyingParameters
    {
        public Vector2 BehaviourDurationRange;
        [Range(0f, 1f)] public float LowFlyingPreference;
        [Range(0f, 1f)] public float LandingPreference;

        [Header("Wander Force")]
        public float WanderForceUpdateIntervalSecs;
        public float SpeedLimit;
        public float SteerForceLimit;
        public float WanderRingDistance;
        public float WanderRingRadius;
    }

    [SerializeField]
    public struct HighLandingParameters
    {
        public float SnapToTargetDistance;
        public float LandingTimeoutSecs;
        public float FlockLandingAreaRadius;
        public float SpeedLimit;
        public float SteerForceLimit;
    }
}
