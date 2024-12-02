using System;
using UnityEngine;

[Serializable]
public class ShelteredState : IBirdState
{
    [SerializeField] private Vector2 _shelteredDurationRange = new Vector2(5f, 20f);

    public void Enter(BirdBrain bird)
    {
        bird.Renderer.enabled = false;
        bird.TargetBirdSpot.OnBirdEntry(bird);
        bird.BehaviorDuration = UnityEngine.Random.Range(_shelteredDurationRange.x, _shelteredDurationRange.y);
        bird.LeafSplash.Play();
    }

    public void Exit(BirdBrain bird)
    {
        bird.Renderer.enabled = true;
        bird.TargetBirdSpot.OnBirdExit(bird);
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