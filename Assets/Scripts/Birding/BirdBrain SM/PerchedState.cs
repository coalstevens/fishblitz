using System;
using UnityEngine;

[Serializable]
public class PerchedState : IBirdState
{
    [SerializeField] private Vector2 _perchedDurationRange = new Vector2(5f, 20f);

    public void Enter(BirdBrain bird)
    {
        if (bird.LandingTargetSpot == null)
        {
            Debug.LogError("LandingTargetSpot is null.");
            return;
        }
        
        bird.LandingTargetSpot.OnBirdEntry(bird);
        bird.Animator.Play("Idle");
        bird.BehaviorDuration = UnityEngine.Random.Range(_perchedDurationRange.x, _perchedDurationRange.y);

        bird.BirdCollider.isTrigger = true;
        bird.SpriteSorting.enabled = false;
        bird.Renderer.sortingLayerName = "Main";
        bird.Renderer.sortingOrder = (bird.LandingTargetSpot as IPerchable).GetSortingOrder() + 2; // +2 to make room for shadow as well, between the two
    }

    public void Exit(BirdBrain bird)
    {
        bird.LandingTargetSpot.OnBirdExit(bird);
    }

    public void Update(BirdBrain bird)
    {
        if (bird.TickAndCheckBehaviorTimer())
        {
            if (bird.PreviousBirdState is LandingState)
                bird.TransitionToState(bird.Flying);
            else if (bird.PreviousBirdState is SoaringLandingState)
                bird.TransitionToState(bird.Soaring);
            else
            {
                Debug.LogError($"Unexpected code path. Previous state: {bird.PreviousBirdState}");
                bird.TransitionToState(bird.Flying);
            }
            return;
        }
    }
}