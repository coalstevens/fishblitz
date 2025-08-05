using System;
using UnityEngine;

public partial class BirdBrain : MonoBehaviour {
    public class FleeingState : IBirdState
    {
        [Header("State Monitoring")]
        [SerializeField] private Collider2D _playerCollider;
        [SerializeField] private Vector2 _fleeForce;
        [SerializeField] private Vector2 _avoidanceForce;
        [SerializeField] private Vector2 _gizAvoidTarget;

        public void Enter(BirdBrain bird)
        {
            BirdBehaviourConfig.FleeingParameters parameters = bird.Config.Fleeing;
            bird._animator.PlayFlying();
            _playerCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>();
            _fleeForce = GetFleeDirection(bird) * parameters.FleeForceMagnitude;
            bird._behaviorDuration = UnityEngine.Random.Range(parameters.BehaviourDurationRangeSecs.x, parameters.BehaviourDurationRangeSecs.y);
            
            bird._birdCollider.isTrigger = false;
            bird._spriteSorting.enabled = true;
            bird._sortingGroup.sortingLayerName = "Main";
        }

        public void Exit(BirdBrain bird)
        {
            // do nothing
        }

        public void Update(BirdBrain bird)
        {
            BirdBehaviourConfig.FleeingParameters parameters = bird.Config.Fleeing;
            if (bird.HasBehaviorTimerElapsed())
            {
                bird.TransitionToState(bird.LowFlying);
                return;
            }

            _avoidanceForce = BirdForces.CalculateAvoidanceForce(
                bird,
                parameters.CircleCastRadius,
                parameters.CircleCastRange,
                parameters.AvoidLayers,
                parameters.AvoidanceWeight,
                out _gizAvoidTarget);

            bird._rb.AddForce(_fleeForce + _avoidanceForce);
            bird._rb.linearVelocity = Vector2.ClampMagnitude(bird._rb.linearVelocity, parameters.FleeMaxSpeed);
        }

        private Vector2 GetFleeDirection(BirdBrain bird) {
            return (Vector2) (bird.transform.position -  _playerCollider.transform.position).normalized;
        }

        public void DrawGizmos(BirdBrain bird)
        {
            Vector2 origin = bird.transform.position;
            float visualScaling = 5f;
            float dotSize = 0.1f;

            // Flee from player
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, _playerCollider.transform.position);

            // Avoid
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + _avoidanceForce * visualScaling);
            Gizmos.DrawSphere(_gizAvoidTarget, dotSize);
        }
    }
}