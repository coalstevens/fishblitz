using System;
using NUnit.Framework;
using UnityEngine;
public partial class BirdBrain : MonoBehaviour {
    public class ShelteredState : IBirdState
    {
        private RigidbodyConstraints2D _originalConstraints;
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
            
            bird._rb.excludeLayers |= bird._highObstacles;
            foreach (var renderer in bird._sortingGroup.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = false;

            _originalConstraints = bird._rb.constraints;
            bird._rb.constraints = RigidbodyConstraints2D.FreezeAll; // lock in place
            bird._sortingGroup.enabled = false;
        }

        public void Exit(BirdBrain bird)
        {
            bird._rb.excludeLayers &= ~(bird._highObstacles | ~bird._lowObstacles); // this exclusion is set in landing state entry
            bird._rb.constraints = _originalConstraints;
            foreach (var renderer in bird._sortingGroup.GetComponentsInChildren<SpriteRenderer>())
                renderer.enabled = true;
            bird._leafSplash.Play();
            bird.LandingTargetSpot.OnBirdExit(bird);
        }

        public void FixedUpdate(BirdBrain bird)
        {
            
            if (bird.HasBehaviorTimerElapsed())
                bird.TransitionToState(bird.LowFlying);
        }
    }
}