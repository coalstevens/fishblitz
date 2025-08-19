using System;
using System.Collections;
using UnityEngine;

public partial class BirdBrain : MonoBehaviour
{
    public class PerchedState : IBirdState
    {
        private RigidbodyConstraints2D _originalConstraints;

        public void DrawGizmos(BirdBrain bird)
        {
        }

        public void Enter(BirdBrain bird)
        {
            if (bird.LandingTargetSpot == null)
            {
                Debug.LogError("LandingTargetSpot is null.");
                return;
            }

            bird.LandingTargetSpot.OnBirdEntry(bird);
            bird._animator.PlayIdle();
            bird._behaviorDuration = UnityEngine.Random.Range(bird.Config.Perched.BehaviourDurationRangeSecs.x, bird.Config.Perched.BehaviourDurationRangeSecs.y);
            _originalConstraints = bird._rb.constraints;
            bird._rb.constraints = RigidbodyConstraints2D.FreezeAll; // lock in place

            bird._sortingGroup.sortingLayerName = "Main";
        }

        public void Exit(BirdBrain bird)
        {
            bird._rb.constraints = _originalConstraints;
            bird.LandingTargetSpot.OnBirdExit(bird);
        }

        public void FixedUpdate(BirdBrain bird)
        {
            if (bird.HasBehaviorTimerElapsed())
            {
                if (bird._previousBirdState is LandingState)
                    bird.TransitionToState(bird.LowFlying);
                else if (bird._previousBirdState is HighLandingState)
                    bird.TransitionToState(bird.HighFlying);
                else
                {
                    Debug.LogError($"Unexpected code path. Previous state: {bird._previousBirdState}");
                    bird.TransitionToState(bird.LowFlying);
                }
                return;
            }
        }
    }
}