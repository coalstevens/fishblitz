using UnityEngine;

public partial class BirdBrain : MonoBehaviour
{
    [System.Serializable]
    public class GroundedState : IBirdState
    {
        public enum TravelMethod { TWO_HOP, WALK };
        private enum TravelState { GROUND, WATER }
        private float _moveDuration = 0f;
        private float _timeUntilNextMove = 0f;
        private float _timeSinceMove = 0;
        private Vector2 _moveForce = Vector2.zero;
        private TravelState _travelState = TravelState.GROUND;
        private bool _isIdle = false;

        public void DrawGizmos(BirdBrain bird)
        {
        }

        public void Enter(BirdBrain bird)
        {
            bird._animator.PlayIdle();
            var _durationRange = bird.Config.Grounded.BehaviourDurationRangeSecs;
            bird._behaviorDuration = Random.Range(_durationRange.x, _durationRange.y);

            bird._rb.includeLayers |= bird._groundObstacle;
            if (!bird.Config.Grounded.CanSwim)
                bird._rb.includeLayers |= bird._water;
            bird._rb.linearDamping = bird._groundedDrag;
            bird._sortingGroup.sortingLayerName = "Main";

            ResetMoveTimer(bird);
        }

        public void Exit(BirdBrain bird)
        {
            bird._rb.includeLayers &= ~bird._groundObstacle; // Disable ground obstacle collision
            bird._rb.includeLayers &= ~bird._water; // Disable water collision
        }

        public void FixedUpdate(BirdBrain bird)
        {
            // Behaviour Timer
            if (bird.HasBehaviorTimerElapsed())
            {
                bird.TransitionToState(bird.LowFlying);
                return;
            }

            // Water check
            if (bird.Config.Grounded.CanSwim)
                _travelState = IsOnWater(bird) ? TravelState.WATER : TravelState.GROUND;
            else
                _travelState = TravelState.GROUND;

            // Movement
            _timeSinceMove += Time.fixedDeltaTime;
            if (_timeSinceMove < _moveDuration)
            {
                ContinueMoveForState(bird);
            }
            else if (_timeSinceMove >= _moveDuration && _timeSinceMove < _timeUntilNextMove)
            {
                PlayIdleForState(bird);
            }
            else
            {
                _isIdle = false;
                StartNextMovement(bird);
                ResetMoveTimer(bird);
            }
        }

        private void ContinueMoveForState(BirdBrain bird)
        {
            if (_travelState == TravelState.GROUND && bird.Config.Grounded.LandTravelType == TravelMethod.TWO_HOP)
                return;

            bird._rb.AddForce(_moveForce, ForceMode2D.Force);
        }

        private void PlayIdleForState(BirdBrain bird)
        {
            if (!_isIdle) bird._rb.linearVelocity = Vector2.zero; // runs once 

            if (_travelState == TravelState.GROUND)
                bird._animator.PlayIdle();
            else if (_travelState == TravelState.WATER)
                bird._animator.PlayIdleSwimming();
            else
                Debug.LogError("Unexpected code path. Unhandled state.");
            _isIdle = true;
        }

        private void StartNextMovement(BirdBrain bird)
        {
            if (_travelState == TravelState.GROUND)
            {
                if (bird.Config.Grounded.LandTravelType == TravelMethod.TWO_HOP)
                    StartTwoHop(bird);
                else if (bird.Config.Grounded.LandTravelType == TravelMethod.WALK)
                    StartWalking(bird);
                else
                    Debug.LogError("Unexpected code path. Unhandled state.");
            }
            else if (_travelState == TravelState.WATER)
            {
                StartSwimming(bird);
            }
            else
                Debug.LogError("Unexpected code path. Unhandled state.");
        }

        private void StartSwimming(BirdBrain bird)
        {
            var parameters = bird.Config.Grounded;

            bird._animator.PlaySwimming();
            _moveDuration = Random.Range(
                parameters.WalkSwimDurationSecs.x,
                parameters.WalkSwimDurationSecs.y);
            _moveForce = Random.insideUnitCircle.normalized * parameters.SwimSpeed;
        }

        private void StartWalking(BirdBrain bird)
        {
            var parameters = bird.Config.Grounded;

            bird._animator.PlayWalking();
            _moveDuration = Random.Range(
                parameters.WalkSwimDurationSecs.x,
                parameters.WalkSwimDurationSecs.y);
            _moveForce = Random.insideUnitCircle.normalized * parameters.WalkSpeed;
        }

        private void StartTwoHop(BirdBrain bird)
        {
            var _hopForceRange = bird.Config.Grounded.TwoHopForceLimits;
            _moveDuration = 0; // Two hop is triggered in a single impulse

            bird._animator.PlayTwoHop();
            Vector2 _hopForce = Random.insideUnitCircle.normalized * Random.Range(_hopForceRange.x, _hopForceRange.y);
            bird._rb.AddForce(_hopForce, ForceMode2D.Impulse);
        }

        private void ResetMoveTimer(BirdBrain bird)
        {
            _timeSinceMove = 0;
            var timeTillMoveLimits = bird.Config.Grounded.TimeTillMoveRangeSecs;
            _timeUntilNextMove = Random.Range(timeTillMoveLimits.x, timeTillMoveLimits.y);
        }

        private bool IsOnWater(BirdBrain bird)
        {
            foreach (var tilemap in bird._waterTilemaps)
            {
                Vector3Int birdTilePos = tilemap.WorldToCell(bird.transform.position);
                if (tilemap.HasTile(birdTilePos))
                    return true;
            }
            return false;
        }
    }
}