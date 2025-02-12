using System;
using UnityEngine;

public partial class BirdBrain : MonoBehaviour {
    [Serializable]
    public class SoaringState : IBirdState
    {
        [SerializeField] private Vector2 _soaringDurationRange = new Vector2(10f, 30f);
        [Range(0f, 1f)][SerializeField] private float _flyingPreference = 0.5f;
        [Range(0f, 1f)][SerializeField] private float _landingPreference = 0.5f;

        [Header("Wander Force")]
        [SerializeField] private float _speedLimit = 1.5f;
        [SerializeField] private float _steerForceLimit = 4f;
        [SerializeField] private float _wanderRingDistance = 4f;
        [SerializeField] private float _wanderRingRadius = 3f;
        private Vector2 _wanderForce = Vector2.zero;

        public void Enter(BirdBrain bird)
        {
            bird._animator.Play("Flying");
            bird.BehaviorDuration = UnityEngine.Random.Range(_soaringDurationRange.x, _soaringDurationRange.y);
            bird._birdCollider.isTrigger = true;
            bird._spriteSorting.enabled = false;
            bird._renderer.sortingLayerName = "Foreground";
        }

        public void Exit(BirdBrain bird)
        {
            // do nothing
        }

        public void Update(BirdBrain bird)
        {
            if (bird.TickAndCheckBehaviorTimer())
            {
                TransitionToPreferredState(bird);
                return;
            }

            _wanderForce = BirdForces.CalculateWanderForce(bird, _speedLimit, _steerForceLimit, _wanderRingDistance, _wanderRingRadius);
            bird._rb.AddForce(_wanderForce);
            bird._rb.linearVelocity = Vector2.ClampMagnitude(bird._rb.linearVelocity, _speedLimit);
        }

        private void TransitionToPreferredState(BirdBrain bird)
        {
            float _randomValue = UnityEngine.Random.Range(0, _flyingPreference + _landingPreference);
            if (_randomValue < _flyingPreference)
                bird.TransitionToState(bird.Flying);
            else if (_randomValue <= _flyingPreference + _landingPreference)
                bird.TransitionToState(bird.SoarLanding);
            else
                bird.TransitionToState(bird.Soaring);
        }
    }
}