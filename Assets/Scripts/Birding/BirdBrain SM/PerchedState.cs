using System;
using UnityEngine;

[Serializable]
public class PerchedState : IBirdState
{
    [SerializeField] private Vector2 _perchedDurationRange = new Vector2(5f, 20f);

    public void Enter(BirdBrain bird)
    {
        bird.LandingTargetSpot.OnBirdEntry(bird);
        bird.Animator.Play("Idle");
        bird.SpriteSorting.enabled = false;
        bird.Renderer.sortingOrder = (bird.LandingTargetSpot as IPerchable).GetSortingOrder() + 1;
        bird.BehaviorDuration = UnityEngine.Random.Range(_perchedDurationRange.x, _perchedDurationRange.y);
    }

    public void Exit(BirdBrain bird)
    {
        bird.LandingTargetSpot.OnBirdExit(bird);
        bird.SpriteSorting.enabled = true;
        if (bird.BirdCollider.isTrigger)
            bird.BirdCollider.isTrigger = false;
    }

    public void Update(BirdBrain bird)
    {
        if (bird.TickAndCheckBehaviorTimer())
        {
            bird.TransitionToState(bird.Flying);
            return;
        }
        bird.CheckIfFrightened();
    }
}