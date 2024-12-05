using System;
using UnityEngine;

[Serializable]
public class FleeingState : IBirdState
{
    [SerializeField] private Vector2 _fleeDurationRange = new Vector2(3f, 5f);
    [SerializeField] private float _fleeForceMagnitude = 2f;
    [SerializeField] private float _fleeMaxSpeed = 3f;
    private Collider2D _playerCollider;
    private Vector2 _fleeForce;

    public void Enter(BirdBrain bird)
    {
        _playerCollider = PlayerCondition.Instance.GetComponent<Collider2D>();
        bird.Animator.Play("Flying");
        bird.BehaviorDuration = UnityEngine.Random.Range(_fleeDurationRange.x, _fleeDurationRange.y);
        _fleeForce = GetFleeDirection(bird) * _fleeForceMagnitude;
    }

    public void Exit(BirdBrain bird)
    {
        // do nothing
    }

    public void Update(BirdBrain bird)
    {
        if (bird.TickAndCheckBehaviorTimer())
        {
            bird.TransitionToState(bird.Flying);
            return;
        }
        bird.RigidBody.AddForce(_fleeForce);
        bird.RigidBody.velocity = Vector2.ClampMagnitude(bird.RigidBody.velocity, _fleeMaxSpeed);
    }

    private Vector2 GetFleeDirection(BirdBrain bird) {
        return (Vector2) (bird.transform.position -  _playerCollider.transform.position).normalized;
    }
}