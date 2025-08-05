using System;
using NUnit.Framework;
using UnityEngine;
public partial class BirdBrain : MonoBehaviour {
    public class ShelteredState : IBirdState
    {
        public void DrawGizmos(BirdBrain bird)
        {
        }

        public void Enter(BirdBrain bird)
        {
            Assert.IsNotNull(bird.LandingTargetSpot, "LandingTargetSpot is null in ShelteredState.Enter");

            bird.LandingTargetSpot.OnBirdEntry(bird);
            
            bird._behaviorDuration = UnityEngine.Random.Range(bird.Config.Sheltered.BehaviourDurationRangeSecs.x, bird.Config.Sheltered.BehaviourDurationRangeSecs.y);
            bird._leafSplashRenderer.sortingOrder = bird.LandingTargetSpot.GetSortingOrder() + 1;
            bird._leafSplash.Play();
            
            bird._sortingGroup.enabled = false;
            bird._birdCollider.isTrigger = true;
            bird._spriteSorting.enabled = false;
            bird._sortingGroup.sortingLayerName = "Main";
        }

        public void Exit(BirdBrain bird)
        {
            bird._sortingGroup.enabled = true;
            bird._leafSplash.Play();
            bird.LandingTargetSpot.OnBirdExit(bird);
        }

        public void Update(BirdBrain bird)
        {
            if (bird.HasBehaviorTimerElapsed())
                bird.TransitionToState(bird.LowFlying);
        }
    }
}