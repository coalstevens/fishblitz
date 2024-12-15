using System;
using UnityEngine;

public partial class BirdBrain : MonoBehaviour {
    [Serializable]
    public class GroundedState : IBirdState
    {
        [SerializeField] private Vector2 _groundedDurationRange = new Vector2(5f, 20f);
        [SerializeField] private Vector2 _timeTillHopLimits = new Vector2(2f, 5f);
        [SerializeField] private Vector2 _twoHopForceLimits = new Vector2(1f, 1f);
        private float _timeUntilNextHop = 0;
        private float _timeSinceHop = 0;

        public void Enter(BirdBrain bird)
        {
            bird._animator.Play("Idle");
            bird.BehaviorDuration = UnityEngine.Random.Range(_groundedDurationRange.x, _groundedDurationRange.y);
            Physics2D.IgnoreLayerCollision(bird.WaterLayer, bird.BirdsLayer, false);

            bird._birdCollider.isTrigger = false;
            bird._spriteSorting.enabled = true;
            bird._renderer.sortingLayerName = "Main";
            ResetHopTimer();
        }

        public void Exit(BirdBrain bird)
        {
            Physics2D.IgnoreLayerCollision(bird.WaterLayer, bird.BirdsLayer, true);
        }

        public void Update(BirdBrain bird)
        {
            if (bird.TickAndCheckBehaviorTimer())
            {
                bird.TransitionToState(bird.Flying);
                return;
            }

            _timeSinceHop += Time.deltaTime;
            if (_timeSinceHop < _timeUntilNextHop)
                return;

            Vector2 _hopForce = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(_twoHopForceLimits.x, _twoHopForceLimits.y);
            bird._rb.AddForce(_hopForce, ForceMode2D.Impulse);
            bird.PlayAnimationThenStop("Two Hop");
            ResetHopTimer();
        }

        private void ResetHopTimer()
        {
            _timeSinceHop = 0;
            _timeUntilNextHop = UnityEngine.Random.Range(_timeTillHopLimits.x, _timeTillHopLimits.y);
        }

    }
}