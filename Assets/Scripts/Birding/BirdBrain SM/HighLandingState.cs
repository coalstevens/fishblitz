using System;
using UnityEngine;

public partial class BirdBrain : MonoBehaviour {
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
            SelectPreferredLandingSpotInLandingCircle(bird); // Fly to a shelter, perch, ground, etc
            _landingStartTime = Time.time;

            bird._rb.excludeLayers |= bird._highObstacles;
            bird._rb.excludeLayers |= bird._people;

            bird._sortingGroup.sortingLayerName = "Foreground";
        }

        public void Exit(BirdBrain bird)
        {
            bird._rb.excludeLayers &= ~bird._highObstacles;
            bird._rb.excludeLayers &= ~bird._people;

            bird._rb.linearVelocity = Vector2.zero;
        }

        public void Update(BirdBrain bird)
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
            if (_isLandingCirclePreset) { 
                _isLandingCirclePreset = false;
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
            _isLandingCirclePreset = true;
            _landingCircleRadius = radius;
            _landingCircleCenter = center;
        }

        private void SelectPreferredLandingSpotInLandingCircle(BirdBrain bird)
        {
            if (bird.TrySetLandingSpotOfType<IPerchableHighElevation>(_landingCircleCenter, _landingCircleRadius))
            {
                _stateOnTargetReached = bird.Perched;
                return;
            }
            _stateOnTargetReached = bird.HighFlying;
        }

        public void DrawGizmos(BirdBrain bird)
        {
            throw new NotImplementedException();
        }
    }
}