using System;
using UnityEngine;

[Serializable]
public class PerchedState : IBirdState
{
    [SerializeField] private Vector2 _perchedDurationRange = new Vector2(5f, 20f);

    public void Enter(BirdBrain bird)
    {
        bird.TargetBirdSpot.OnBirdEntry(bird);
        bird.Animator.Play("Idle");
        bird.SpriteSorting.enabled = false;
        bird.Renderer.sortingOrder = (bird.TargetBirdSpot as IPerchable).GetSortingOrder() + 1;
        bird.BehaviorDuration = UnityEngine.Random.Range(_perchedDurationRange.x, _perchedDurationRange.y);
    }

    public void Exit(BirdBrain bird)
    {
        bird.TargetBirdSpot.OnBirdExit(bird);
        bird.SpriteSorting.enabled = true;
    }

    public void Update(BirdBrain bird)
    {
        if (bird.IsBehaviourDurationExpired())
        {
            bird.TransitionToState(bird.FlyingState);
            return;
        }
        bird.CheckIfFrightened();
    }
}