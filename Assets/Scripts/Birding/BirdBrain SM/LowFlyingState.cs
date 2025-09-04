using System;
using UnityEngine;

public partial class BirdBrain : MonoBehaviour
{
    public class LowFlyingState : IBirdState
    {
        [Header("State Monitoring")]
        private float _lastBoidForceUpdateTime = 0;
        private float _lastWanderForceUpdateTime = 0;
        [SerializeField] private Vector2 _boidForce = Vector2.zero;
        [SerializeField] private Vector2 _wanderForce = Vector2.zero;
        [SerializeField] private Vector2 _avoidanceForce = Vector2.zero;
        [SerializeField] private Vector2 _wanderRingCenter;
        [SerializeField] private Vector2 _gizAvoidTarget;

        public void Enter(BirdBrain bird)
        {
            BirdBehaviourConfig.LowFlyingParameters parameters = bird.Config.LowFlying;
            bird._animator.PlayFlying();
            bird._behaviorDuration = UnityEngine.Random.Range(parameters.BehaviourDurationRangeSecs.x, parameters.BehaviourDurationRangeSecs.y);
            bird._sortingGroup.sortingLayerName = "Main";
            bird._rb.linearDamping = bird._flyingDrag;
        }

        public void Exit(BirdBrain bird)
        {
            // do nothing
        }

        public void FixedUpdate(BirdBrain bird)
        {
            BirdBehaviourConfig.LowFlyingParameters parameters = bird.Config.LowFlying;

            if (bird.HasBehaviorTimerElapsed())
            {
                TransitionToPreferredState(bird, parameters);
            }
            else
            {
                if (Time.time - _lastWanderForceUpdateTime >= parameters.WanderForceUpdateIntervalSecs)
                {
                    _wanderForce = BirdForces.CalculateWanderForce(
                        bird,
                        parameters.SpeedLimit,
                        parameters.SteerForceLimit,
                        parameters.WanderRingDistance,
                        parameters.WanderRingRadius,
                        out _wanderRingCenter);

                    _lastWanderForceUpdateTime = Time.time;
                }
                if (Time.time - _lastBoidForceUpdateTime >= parameters.BoidForceUpdateIntervalSecs)
                {
                    _boidForce = BirdForces.CalculateBoidForce(
                        bird,
                        parameters.BoidForceWeight,
                        parameters.MaxFlockMates, parameters.SeparationWeight,
                        parameters.AlignmentWeight,
                        parameters.CohesionWeight);
                    _lastBoidForceUpdateTime = Time.time;
                }
                _avoidanceForce = BirdForces.CalculateAvoidanceForce(
                    bird,
                    parameters.CircleCastRadius,
                    parameters.CircleCastRange,
                    parameters.AvoidanceWeight,
                    out _gizAvoidTarget);
            }
            bird._rb.AddForce(_wanderForce + _boidForce + _avoidanceForce);
            bird._rb.linearVelocity = Vector2.ClampMagnitude(bird._rb.linearVelocity, parameters.SpeedLimit);
        }

        private void TransitionToPreferredState(BirdBrain bird, BirdBehaviourConfig.LowFlyingParameters parameters)
        {
            float _randomValue = UnityEngine.Random.Range(0, parameters.LandingPreference + parameters.HighFlyingPreference);
            if (_randomValue < parameters.LandingPreference)
            {
                bird.Landing.SetLandingTargetArea(parameters.WanderRingRadius, _wanderRingCenter);
                bird.TransitionToState(bird.Landing);
            }
            else
            {
                bird.TransitionToState(bird.HighFlying);
            }
        }

        public void DrawGizmos(BirdBrain bird)
        {
            BirdBehaviourConfig.LowFlyingParameters parameters = bird.Config.LowFlying;
            Vector2 origin = bird.transform.position;
            float visualScaling = 5f;
            float dotSize = 0.1f;

            // Wander
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(bird.TargetPosition, dotSize);
            Gizmos.DrawWireSphere(_wanderRingCenter, parameters.WanderRingRadius);
            Gizmos.DrawLine(origin, origin + _wanderForce * visualScaling);

            // Avoid
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + _avoidanceForce * visualScaling);
            Gizmos.DrawSphere(_gizAvoidTarget, dotSize);

            // Flock
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + _boidForce * visualScaling);
        }
    }
}