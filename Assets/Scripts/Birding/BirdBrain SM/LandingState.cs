using UnityEngine;
using UnityEngine.Tilemaps;

public partial class BirdBrain : MonoBehaviour
{
    public class LandingState : IBirdState
    {
        private float _landingStartTime;
        private IBirdState _stateOnTargetReached;
        private Vector2 _landingTargetAreaCenter;
        private float _landingTargetAreaRadius;
        private bool _landingCirclePreset = false;
        private Vector2 _avoidanceForce;
        private Vector2 _seekForce;
        private Vector2 _gizAvoidTarget;

        // Stuck check 
        private Vector2 _lastCheckedPosition;
        private float _lastPositionCheckTime;

        public void Enter(BirdBrain bird)
        {
            bird._animator.PlayGliding();
            SetLandingCircle(bird);
            _stateOnTargetReached = SetPreferredLandingSpotInLandingArea(bird);
            _landingStartTime = Time.time;

            // Disabling collisions so bird doesn't fly into sheltering object
            if (_stateOnTargetReached == bird.Sheltered)
                bird._rb.excludeLayers |= bird._highObstacles | bird._lowObstacles; // this is reversed in sheltered state exit
            bird._rb.linearDamping = bird._flyingDrag; 
            bird._spriteSorting.enabled = true;
            bird._sortingGroup.sortingLayerName = "Main";
        }

        public void Exit(BirdBrain bird)
        {
            _landingCirclePreset = false;
        }

        public void FixedUpdate(BirdBrain bird)
        {
            var parameters = bird.Config.LowLanding;

            // No need to get to the target if trying to fly
            if (_stateOnTargetReached == bird.LowFlying)
            {
                bird.TransitionToState(_stateOnTargetReached);
                return;
            }

            CheckandWarpIfStuck(bird);
            CheckAndWarpIfNearTarget(bird);

            _avoidanceForce = BirdForces.CalculateAvoidanceForce(
                bird,
                parameters.CircleCastRadius,
                parameters.CircleCastRange,
                parameters.AvoidanceWeight,
                out _gizAvoidTarget);

            _seekForce = BirdForces.Seek(bird, parameters.SpeedLimit, parameters.SteerForceLimit);

            bird._rb.AddForce(_avoidanceForce + _seekForce);
        }

        private void CheckAndWarpIfNearTarget(BirdBrain bird)
        {
            var parameters = bird.Config.LowLanding;
            // Teleport if the bird is close enough to the target
            if (Vector2.Distance(bird.TargetPosition, bird.transform.position) <= parameters.SnapToTargetDistance)
            {
                bird.transform.position = bird.TargetPosition;
                bird._rb.linearVelocity = Vector2.zero;
                bird.TransitionToState(_stateOnTargetReached);
                return;
            }
        }

        private void CheckandWarpIfStuck(BirdBrain bird)
        {
            var parameters = bird.Config.LowLanding;
            LayerMask obstacles = bird._highObstacles | bird._lowObstacles; // Bird should be clearing ground and water in this state already
            float clearanceDistance = 0.05f; // small offset to avoid spawning inside  
            float timeSinceLastMove = Time.time - _lastPositionCheckTime;
            float distanceMoved = Vector2.Distance(bird.transform.position, _lastCheckedPosition);

            // Bird hasn't moved much for timeout period, consider stuck
            if (timeSinceLastMove >= parameters.LandingTimeoutSecs && distanceMoved <= parameters.StuckMovementThreshold)
            {
                Vector2 origin = bird.transform.position;
                Vector2 direction = (bird.TargetPosition - origin).normalized;
                float maxDistance = Vector2.Distance(origin, bird.TargetPosition);

                RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, obstacles);

                if (hit.collider != null)
                {
                    // Warp to the other side of the collider, along the ray direction
                    Vector2 warpPosition = hit.point + direction * (hit.collider.bounds.extents.magnitude + clearanceDistance);

                    bird.transform.position = warpPosition;
                    return;
                }
            }

            // check if bird moved 
            if (distanceMoved > parameters.StuckMovementThreshold)
            {
                _lastCheckedPosition = bird.transform.position;
                _lastPositionCheckTime = Time.time;
            }
        }

        private void SetLandingCircle(BirdBrain bird)
        {
            // Use the preset landing circle if it exists
            if (_landingCirclePreset)
            {
                _landingCirclePreset = false;
                return;
            }

            // Default to use the ViewDistanceCollider
            if (bird.ViewDistance is CircleCollider2D circle)
                SetLandingTargetArea(circle.radius, (Vector2)circle.transform.position + circle.offset);
            else
                Debug.LogError("ViewDistance on bird must be a circle collider");
        }

        public void SetLandingTargetArea(float radius, Vector2 center)
        {
            _landingCirclePreset = true;
            _landingTargetAreaRadius = radius;
            _landingTargetAreaCenter = center;
        }

        // Fly to a shelter, perch, ground, etc
        private IBirdState SetPreferredLandingSpotInLandingArea(BirdBrain bird)
        {
            BirdBehaviourConfig.LowLandingParameters parameters = bird.Config.LowLanding;
            float _randomValue = UnityEngine.Random.Range(0f, parameters.PerchPreference + parameters.ShelterPreference + parameters.GroundPreference);
            if (_randomValue < parameters.PerchPreference)
            {
                if (bird.TrySetLandingSpotOfType<IPerchableLowElevation>(_landingTargetAreaCenter, _landingTargetAreaRadius))
                {
                    return bird.Perched;
                }
            }
            else if (_randomValue < parameters.PerchPreference + parameters.ShelterPreference)
            {
                if (bird.TrySetLandingSpotOfType<IShelterable>(_landingTargetAreaCenter, _landingTargetAreaRadius))
                {
                    return bird.Sheltered;
                }
            }
            else if (_randomValue <= parameters.PerchPreference + parameters.ShelterPreference + parameters.GroundPreference)
            {
                // X tries to find a valid landing spot 
                for (int i = 0; i < 5; i++)
                {
                    if (!IsTargetOverWater(bird.TargetPosition) && !IsObstacleInTheWay(bird, bird.TargetPosition))
                    {
                        return bird.Grounded;
                    }
                    bird.TargetPosition = GeneratePointInLandingCircle(bird);
                }
            }
            return bird.LowFlying;
        }

        private Vector2 GeneratePointInLandingCircle(BirdBrain bird)
        {
            return _landingTargetAreaCenter + UnityEngine.Random.insideUnitCircle * _landingTargetAreaRadius;
        }

        private bool IsObstacleInTheWay(BirdBrain bird, Vector2 targetPosition)
        {
            return Physics2D.Linecast(bird.transform.position, targetPosition, bird._lowObstacles | bird._highObstacles);
        }

        private bool IsTargetOverWater(Vector2 targetPosition)
        {
            Tilemap[] _tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            foreach (Tilemap _tilemap in _tilemaps)
            {
                if (IsPositionWithinTilemap(_tilemap, targetPosition))
                {
                    string _layerName = LayerMask.LayerToName(_tilemap.gameObject.layer);
                    if (_layerName == "Water")
                        return true;
                }
            }
            return false;
        }

        private bool IsPositionWithinTilemap(Tilemap tilemap, Vector2 worldPosition)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
            return tilemap.GetTile(cellPosition) != null;
        }

        public void DrawGizmos(BirdBrain bird)
        {
            Vector2 origin = bird.transform.position;
            float dotSize = 0.1f;
            float visualScaling = 5f;

            // Landing
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_landingTargetAreaCenter, _landingTargetAreaRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(bird.transform.position, bird.TargetPosition);
            Gizmos.DrawSphere(bird.TargetPosition, dotSize);

            // Avoid
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + _avoidanceForce * visualScaling);
            Gizmos.DrawSphere(_gizAvoidTarget, dotSize);
        }
    }
}