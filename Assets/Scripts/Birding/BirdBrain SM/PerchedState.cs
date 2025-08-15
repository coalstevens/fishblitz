using System;
using System.Collections;
using UnityEngine;

public partial class BirdBrain : MonoBehaviour
{
    public class PerchedState : IBirdState
    {
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

            bird._sortingGroup.sortingLayerName = "Main";
        }

        public void Exit(BirdBrain bird)
        {
            bird.LandingTargetSpot.OnBirdExit(bird);
        }

        public void Update(BirdBrain bird)
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