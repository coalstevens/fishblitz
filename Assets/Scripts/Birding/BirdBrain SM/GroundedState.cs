using System;
using UnityEngine;

public partial class BirdBrain : MonoBehaviour
{
    public class GroundedState : IBirdState
    {
        private float _timeUntilNextHop = 0;
        private float _timeSinceHop = 0;

        public void DrawGizmos(BirdBrain bird)
        {
        }

        public void Enter(BirdBrain bird)
        {
            bird._animator.PlayIdle();
            var _durationRange = bird.Config.Grounded.BehaviourDurationRangeSecs;
            bird._behaviorDuration = UnityEngine.Random.Range(_durationRange.x, _durationRange.y);

            bird._rb.includeLayers |= bird._groundObstacle; 
            bird._rb.includeLayers |= bird._water; 

            bird._sortingGroup.sortingLayerName = "Main";
            ResetHopTimer(bird);
        }

        public void Exit(BirdBrain bird)
        {
            bird._rb.includeLayers &= ~bird._groundObstacle; // Disable ground obstacle collision
            bird._rb.includeLayers &= ~bird._water; // Disable water collision
        }

        public void Update(BirdBrain bird)
        {
            if (bird.HasBehaviorTimerElapsed())
            {
                bird.TransitionToState(bird.LowFlying);
                return;
            }

            _timeSinceHop += Time.fixedDeltaTime;
            if (_timeSinceHop < _timeUntilNextHop)
                return;
            var _hopForceRange = bird.Config.Grounded.TwoHopForceLimits;
            Vector2 _hopForce = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(_hopForceRange.x, _hopForceRange.y);
            bird._rb.AddForce(_hopForce, ForceMode2D.Impulse);
            bird._animator.PlayTwoHop();
            ResetHopTimer(bird);
        }

        private void ResetHopTimer(BirdBrain bird)
        {
            _timeSinceHop = 0;
            var _timeTillHopLimits = bird.Config.Grounded.TimeTillHopRangeSecs;
            _timeUntilNextHop = UnityEngine.Random.Range(_timeTillHopLimits.x, _timeTillHopLimits.y);
        }

    }
}