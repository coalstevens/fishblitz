using UnityEngine;

public partial class BirdBrain : MonoBehaviour
{
    public class HighFlyingState : IBirdState
    {
        [Header("State Monitoring")]
        [SerializeField] private Vector2 _wanderForce = Vector2.zero;
        [SerializeField] private Vector2 _wanderRingCenter;
        private float _lastWanderForceUpdateTime;

        public void Enter(BirdBrain bird)
        {
            var parameters = bird.Config.HighFlying;
            bird._animator.PlayFlying();
            bird._behaviorDuration = Random.Range(parameters.BehaviourDurationRange.x, parameters.BehaviourDurationRange.y);

            bird._rb.excludeLayers |= bird._lowObstacles;
            bird._rb.excludeLayers |= bird._people;
            bird._rb.linearDamping = bird._flyingDrag; 

            bird._sortingGroup.sortingLayerName = "Foreground";
        }

        public void Exit(BirdBrain bird)
        {
            bird._rb.excludeLayers &= ~bird._lowObstacles;
            bird._rb.excludeLayers &= ~bird._people;
        }

        public void FixedUpdate(BirdBrain bird)
        {
            var parameters = bird.Config.HighFlying;

            if (bird.HasBehaviorTimerElapsed())
            {
                TransitionToPreferredState(bird, parameters);
                return;
            }

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

            bird._rb.AddForce(_wanderForce);
            bird._rb.linearVelocity = Vector2.ClampMagnitude(bird._rb.linearVelocity, parameters.SpeedLimit);
        }

        private void TransitionToPreferredState(BirdBrain bird, BirdBehaviourConfig.HighFlyingParameters parameters)
        {
            float _randomValue = Random.Range(0, parameters.LowFlyingPreference + parameters.LandingPreference);
            if (_randomValue < parameters.LowFlyingPreference)
                bird.TransitionToState(bird.LowFlying);
            else if (_randomValue <= parameters.LowFlyingPreference + parameters.LandingPreference)
            {
                bird.Landing.SetLandingTargetArea(parameters.WanderRingRadius, _wanderRingCenter);
                bird.TransitionToState(bird.HighLanding);
            }
            else
                bird.TransitionToState(bird.HighFlying);
        }

        public void DrawGizmos(BirdBrain bird)
        {
            BirdBehaviourConfig.HighFlyingParameters parameters = bird.Config.HighFlying;
            Vector2 origin = bird.transform.position;
            float visualScaling = 5f;
            float dotSize = 0.1f;

            // Wander
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(bird.TargetPosition, dotSize);
            Gizmos.DrawWireSphere(_wanderRingCenter, parameters.WanderRingRadius);
            Gizmos.DrawLine(origin, origin + _wanderForce * visualScaling);
        }
    }
}