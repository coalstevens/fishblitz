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

        public void Enter(BirdBrain bird)
        {
            bird._animator.PlayGliding();
            SetLandingCircle(bird);
            _stateOnTargetReached = SetPreferredLandingSpotInLandingArea(bird);
            _landingStartTime = Time.time;

            // Disabling collisions in some cases so that bird doesn't run into the object its trying to land on
            bird._birdCollider.isTrigger = (_stateOnTargetReached == bird.Sheltered || _stateOnTargetReached == bird.Perched);
            bird._spriteSorting.enabled = true;
            bird._sortingGroup.sortingLayerName = "Main";
        }

        public void Exit(BirdBrain bird)
        {
            bird._rb.linearVelocity = Vector2.zero;
        }

        public void Update(BirdBrain bird)
        {
            var parameters = bird.Config.LowLanding;

            // No need to approach the target if trying to fly
            if (_stateOnTargetReached == bird.LowFlying)
            {
                bird.TransitionToState(_stateOnTargetReached);
                return;
            }

            // Teleport if landing is taking too long or if the bird is close enough to the target
            if (Time.time - _landingStartTime >= parameters.LandingTimeoutSecs ||
                Vector2.Distance(bird.TargetPosition, bird.transform.position) <= parameters.SnapToTargetDistance)
            {
                bird.transform.position = bird.TargetPosition;
                bird._rb.linearVelocity = Vector2.zero;
                bird.TransitionToState(_stateOnTargetReached);
                return;
            }

            if (_stateOnTargetReached is GroundedState)
            {
                _avoidanceForce = BirdForces.CalculateAvoidanceForce(
                    bird,
                    parameters.CircleCastRadius,
                    parameters.CircleCastRange,
                    parameters.AvoidLayers,
                    parameters.AvoidanceWeight,
                    out _gizAvoidTarget);
            }
            else
            {
                _avoidanceForce = Vector2.zero;
            }

            _seekForce = BirdForces.Seek(bird, parameters.SpeedLimit, parameters.SteerForceLimit);

            bird._rb.AddForce(_avoidanceForce + _seekForce);
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
                // X tries to find a ground spot not over water
                for (int i = 0; i < 3; i++)
                {
                    if (!IsTargetOverWater(bird.TargetPosition))
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

        private bool IsTargetOverWater(Vector2 birdPosition)
        {
            Tilemap[] _tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            foreach (Tilemap _tilemap in _tilemaps)
            {
                if (IsPositionWithinTilemap(_tilemap, birdPosition))
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