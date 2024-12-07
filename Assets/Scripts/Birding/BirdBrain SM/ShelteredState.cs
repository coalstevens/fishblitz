using System;
using UnityEngine;

[Serializable]
public class ShelteredState : IBirdState
{
    [SerializeField] private Vector2 _shelteredDurationRange = new Vector2(5f, 20f);

    public void Enter(BirdBrain bird)
    {
        bird.LandingTargetSpot.OnBirdEntry(bird);
        bird.BehaviorDuration = UnityEngine.Random.Range(_shelteredDurationRange.x, _shelteredDurationRange.y);
        bird.LeafSplashRenderer.sortingOrder = bird.LandingTargetSpot.GetSortingOrder() + 1;
        bird.LeafSplash.Play();
        
        bird.Renderer.enabled = false;
        bird.BirdCollider.isTrigger = true;
        bird.SpriteSorting.enabled = false;
        bird.Renderer.sortingLayerName = "Main";
    }

    public void Exit(BirdBrain bird)
    {
        bird.Renderer.enabled = true;
        bird.LeafSplash.Play();
        bird.LandingTargetSpot.OnBirdExit(bird);
    }

    public void Update(BirdBrain bird)
    {
        if (bird.TickAndCheckBehaviorTimer())
            bird.TransitionToState(bird.Flying);
    }
}