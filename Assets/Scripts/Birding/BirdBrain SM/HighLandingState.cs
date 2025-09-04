using System;
using UnityEngine;

public partial class BirdBrain : MonoBehaviour
{
    public class HighLandingState : IBirdState
    {
        private float _landingStartTime;
        private IBirdState _stateOnTargetReached;

        // The area in which the bird searches for a landing spot
        private Vector2 _landingCircleCenter;
        private float _landingCircleRadius;
        private bool _isLandingCirclePreset = false;

        public void Enter(BirdBrain bird)
        {
            bird._animator.PlayFlying();
            UpdateLandingCircle(bird);
            _stateOnTargetReached = SetPreferredLandingSpotInLandingArea(bird); // Fly to a shelter, perch, ground, etc
            _landingStartTime = Time.time;

            bird._rb.excludeLayers |= bird._lowObstacles;
            bird._rb.excludeLayers |= bird._people;
            bird._rb.linearDamping = bird._flyingDrag;

            bird._sortingGroup.sortingLayerName = "Foreground";
        }

        public void Exit(BirdBrain bird)
        {
            bird._rb.excludeLayers &= ~bird._lowObstacles;
            bird._rb.excludeLayers &= ~bird._people;

            if (_stateOnTargetReached == bird.Perched)
                bird._rb.linearVelocity = Vector2.zero;
        }

        public void FixedUpdate(BirdBrain bird)
        {
            var parameters = bird.Config.HighLanding;

            if (_stateOnTargetReached == bird.HighFlying)
                bird.TransitionToState(_stateOnTargetReached);

            // Teleport if landing is taking too long or if the bird is close enough to the target
            if (Time.time - _landingStartTime >= parameters.LandingTimeoutSecs ||
                Vector2.Distance(bird.TargetPosition, bird.transform.position) <= parameters.SnapToTargetDistance)
            {
                bird.transform.position = bird.TargetPosition;
                bird._rb.linearVelocity = Vector2.zero;
                bird.TransitionToState(_stateOnTargetReached);
                return;
            }
            // Apply force to approach target
            bird._rb.AddForce(BirdForces.Seek(bird, parameters.SpeedLimit, parameters.SteerForceLimit));
        }

        private void UpdateLandingCircle(BirdBrain bird)
        {
            // Landing circle set externally
            if (_isLandingCirclePreset)
            {
                _isLandingCirclePreset = false;
                return;
            }

            // TODO - change to a circle generated around wandercircle
            // Default is to use ViewDistanceCollider
            if (bird.ViewDistance is CircleCollider2D circle)
                SetLandingCircle(circle.radius, (Vector2)circle.transform.position + circle.offset);
            else
                Debug.LogError("ViewDistance on bird must be a circle collider");
        }

        public void SetLandingCircle(float radius, Vector2 center)
        {
            _isLandingCirclePreset = true;
            _landingCircleRadius = radius;
            _landingCircleCenter = center;
        }

        private IBirdState SetPreferredLandingSpotInLandingArea(BirdBrain bird)
        {
            BirdBehaviourConfig.HighLandingParameters parameters = bird.Config.HighLanding;
            float _randomValue = UnityEngine.Random.Range(0f, parameters.PerchPreference + parameters.GroundPreference);
            if (_randomValue < parameters.PerchPreference)
            {
                if (bird.TrySetLandingSpotOfType<IPerchableHighElevation>(_landingCircleCenter, _landingCircleRadius))
                {
                    return bird.Perched;
                }
            }
            else if (_randomValue <= parameters.PerchPreference + parameters.GroundPreference)
            {
                // X tries to find a valid landing spot 
                for (int i = 0; i < 5; i++)
                {
                    bool obstacleInWay = bird.IsObstacleInTheWay(bird.TargetPosition);
                    bool targetOverWater = bird.IsTargetOverWater(bird.TargetPosition);
                    bool cannotSwim = !bird.Config.Grounded.CanSwim;

                    if (obstacleInWay || (cannotSwim && targetOverWater))
                    {
                        bird.TargetPosition = GeneratePointInLandingCircle(bird);
                        continue;
                    }

                    return bird.Grounded; // Target is valid
                }
            }
            return bird.HighFlying;
        }

        public void DrawGizmos(BirdBrain bird)
        {
            Vector2 origin = bird.transform.position;
            float dotSize = 0.1f;
            // float visualScaling = 5f;

            // Landing
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_landingCircleCenter, _landingCircleRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(bird.transform.position, bird.TargetPosition);
            Gizmos.DrawSphere(bird.TargetPosition, dotSize);

            // Avoid
            // Gizmos.color = Color.red;
            // Gizmos.DrawLine(origin, origin + _avoidanceForce * visualScaling);
            // Gizmos.DrawSphere(_gizAvoidTarget, dotSize);
        }

        private Vector2 GeneratePointInLandingCircle(BirdBrain bird)
        {
            return _landingCircleCenter + UnityEngine.Random.insideUnitCircle * _landingCircleRadius;
        }
    }
}