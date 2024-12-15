using System;
using UnityEngine;
public partial class BirdBrain : MonoBehaviour {
    [Serializable]
    public class ShelteredState : IBirdState
    {
        [SerializeField] private Vector2 _shelteredDurationRange = new Vector2(5f, 20f);

        public void Enter(BirdBrain bird)
        {
            if (bird.LandingTargetSpot == null)
            {
                Debug.LogError("LandingTargetSpot is null.");
                return;
            }
            bird.LandingTargetSpot.OnBirdEntry(bird);
            bird.BehaviorDuration = UnityEngine.Random.Range(_shelteredDurationRange.x, _shelteredDurationRange.y);
            bird._leafSplashRenderer.sortingOrder = bird.LandingTargetSpot.GetSortingOrder() + 1;
            bird._leafSplash.Play();
            
            bird._renderer.enabled = false;
            bird._birdCollider.isTrigger = true;
            bird._spriteSorting.enabled = false;
            bird._renderer.sortingLayerName = "Main";
        }

        public void Exit(BirdBrain bird)
        {
            bird._renderer.enabled = true;
            bird._leafSplash.Play();
            bird.LandingTargetSpot.OnBirdExit(bird);
        }

        public void Update(BirdBrain bird)
        {
            if (bird.TickAndCheckBehaviorTimer())
                bird.TransitionToState(bird.Flying);
        }
    }
}