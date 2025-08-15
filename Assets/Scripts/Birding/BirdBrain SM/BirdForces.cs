using System.Linq;
using UnityEngine;

public partial class BirdBrain : MonoBehaviour
{
    public static class BirdForces
    {
        public static Vector2 CalculateAvoidanceForce(
            BirdBrain bird,
            float circleCastRadius,
            float circleCastRange,
            float avoidanceWeight,
            out Vector2 obstaclePosition)
        {
            obstaclePosition = Vector2.zero;

            if (circleCastRange <= 0 || circleCastRadius <= 0)
            {
                Debug.LogWarning("CircleCast parameters should be greater than zero.");
                return Vector2.zero;
            }

            RaycastHit2D hit = Physics2D.CircleCast(
                bird.transform.position,
                circleCastRadius,
                bird._rb.linearVelocity.normalized,
                circleCastRange,
                GetBirdInteractionLayers(bird)
            );

            if (hit && !hit.collider.isTrigger)
            {
                float _proximityFactor = 1 - hit.fraction; // Closer -> stronger
                Vector2 _avoidanceForce = _proximityFactor * avoidanceWeight * hit.normal; // dir is hit collider normal
                obstaclePosition = hit.point;
                return _avoidanceForce;
            }
            else
            {
                return Vector2.zero;
            }
        }

        public static Vector2 CalculateWanderForce(
            BirdBrain bird,
            float speedLimit,
            float steerForceLimit,
            float wanderRingDistance,
            float wanderRingRadius,
            out Vector2 ringCenter)
        {
            ringCenter = (Vector2)bird.transform.position + bird._rb.linearVelocity.normalized * wanderRingDistance;
            bird.TargetPosition = ringCenter + wanderRingRadius * UnityEngine.Random.insideUnitCircle.normalized;
            return Seek(bird, speedLimit, steerForceLimit);
        }

        public static Vector2 CalculateBoidForce(
            BirdBrain bird,
            float boidForceWeight,
            float maxFlockMates,
            float separationWeight,
            float alignmentWeight,
            float cohesionWeight)
        {
            Vector2 _separation = Vector2.zero; // Prevents birds getting too close
            Vector2 _alignment = Vector2.zero; // Urge to match direction of others
            Vector2 _cohesion = Vector2.zero; // Urge to move towards centroid of flock
            int _count = 0;

            var _nearbyBirds = bird._nearbyBirdsTracker.NearbyBirds
                .Where(b => bird.SpeciesData.FlockableSpecies.Contains(b.SpeciesData))
                .OrderBy(b => Vector2.Distance(bird.transform.position, b.transform.position)); // Sorted so closer birds are selected first

            foreach (var _nearbyBird in _nearbyBirds)
            {
                if (_nearbyBird.gameObject == null) continue;
                float _distance = Vector2.Distance(bird.transform.position, _nearbyBird.transform.position);
                _separation += (Vector2)(bird.transform.position - _nearbyBird.transform.position) / _distance;
                _alignment += _nearbyBird.GetVelocity();
                _cohesion += (Vector2)_nearbyBird.transform.position;
                _count++;

                if (_count >= maxFlockMates)
                    break;
            }

            if (_count > 0)
            {
                _separation /= _count;
                _alignment /= _count;
                _cohesion /= _count;
                _cohesion = (_cohesion - (Vector2)bird.transform.position).normalized;
                Vector2 boidForce =
                    (_separation.normalized * separationWeight +
                    _alignment.normalized * alignmentWeight +
                    _cohesion * cohesionWeight).normalized;
                return boidForce * boidForceWeight;
            }

            return Vector2.zero;
        }

        public static Vector2 Seek(BirdBrain bird, float speedLimit, float steerForceLimit)
        {
            Vector2 _desired = (bird.TargetPosition - (Vector2)bird.transform.position).normalized * speedLimit;
            Vector2 _steer = _desired - bird._rb.linearVelocity;
            if (_steer.magnitude >= steerForceLimit)
                _steer = _steer.normalized * steerForceLimit;

            return _steer;
        }

        public static LayerMask GetBirdInteractionLayers(BirdBrain bird)
        {
            int birdLayer = bird.gameObject.layer;
            int mask = 0;

            // Build base from layer collision matrix
            for (int i = 0; i < 32; i++)
                if (!Physics2D.GetIgnoreLayerCollision(birdLayer, i))
                    mask |= (1 << i);

            mask &= ~(1 << birdLayer); // Ignore the bird layer (so we don't hit self)

            // Apply per instance overrides
            mask &= ~(bird._rb.excludeLayers);
            mask |= bird._rb.includeLayers;

            return mask;
        }
    }
}