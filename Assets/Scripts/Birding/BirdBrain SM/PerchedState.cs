using System;
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
            bird._animator.PlayFlying();
            bird._behaviorDuration = UnityEngine.Random.Range(bird.Config.Perched.BehaviourDurationRangeSecs.x, bird.Config.Perched.BehaviourDurationRangeSecs.y);

            bird._birdCollider.isTrigger = true;
            bird._spriteSorting.enabled = false;
            bird._sortingGroup.sortingLayerName = "Main";
            bird._sortingGroup.sortingOrder = (bird.LandingTargetSpot as IPerchable).GetSortingOrder() + 2; // +2 to make room for shadow as well, between the two
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