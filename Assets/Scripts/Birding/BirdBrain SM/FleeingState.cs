using System;
using UnityEngine;

public partial class BirdBrain : MonoBehaviour {
    [Serializable]
    public class FleeingState : IBirdState
    {
        [SerializeField] private Vector2 _fleeDurationRange = new Vector2(3f, 5f);
        [SerializeField] private float _fleeForceMagnitude = 2f;
        [SerializeField] private float _fleeMaxSpeed = 3f;
        
        [Header("Avoidance Force")]
        [SerializeField] private float _avoidanceWeight = 1.0f;
        [SerializeField] private float _circleCastRadius = 1.0f;
        [SerializeField] private float _circleCastRange = 3.0f;
        private Collider2D _playerCollider;
        private Vector2 _fleeForce;
        private Vector2 _avoidanceForce;

        public void Enter(BirdBrain bird)
        {
            bird._animator.Play("Flying");
            _playerCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>();
            _fleeForce = GetFleeDirection(bird) * _fleeForceMagnitude;
            bird.BehaviorDuration = UnityEngine.Random.Range(_fleeDurationRange.x, _fleeDurationRange.y);
            
            bird._birdCollider.isTrigger = false;
            bird._spriteSorting.enabled = true;
            bird._renderer.sortingLayerName = "Main";
        }

        public void Exit(BirdBrain bird)
        {
            // do nothing
        }

        public void Update(BirdBrain bird)
        {
            if (bird.TickAndCheckBehaviorTimer())
            {
                bird.TransitionToState(bird.Flying);
                return;
            }
            
            _avoidanceForce = BirdForces.CalculateAvoidanceForce(bird, _circleCastRadius, _circleCastRange, _avoidanceWeight);
            bird._rb.AddForce(_fleeForce + _avoidanceForce);
            bird._rb.velocity = Vector2.ClampMagnitude(bird._rb.velocity, _fleeMaxSpeed);
        }

        private Vector2 GetFleeDirection(BirdBrain bird) {
            return (Vector2) (bird.transform.position -  _playerCollider.transform.position).normalized;
        }
    }
}