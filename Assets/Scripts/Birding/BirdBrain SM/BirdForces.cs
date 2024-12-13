using System.Linq;
using UnityEngine;

public static class BirdForces
{
    public static Vector2 CalculateAvoidanceForce(BirdBrain bird, float circleCastRadius, float circleCastRange, float avoidanceWeight)
    {
        if (circleCastRange <= 0 || circleCastRadius <= 0)
        {
            Debug.LogWarning("CircleCast parameters should be greater than zero.");
            return Vector2.zero;
        }

        RaycastHit2D hit = Physics2D.CircleCast(
            bird.transform.position,
            circleCastRadius,
            bird.RigidBody.velocity.normalized,
            circleCastRange
        );

        Vector2 _avoidanceForce = Vector2.zero;
        if (hit.collider != null)
        {
            Vector2 _obstaclePosition = hit.point;
            Vector2 _avoidanceDirection = ((Vector2)bird.transform.position - _obstaclePosition).normalized;
            float _proximityFactor = 1 - (hit.distance / circleCastRange); // Closer -> stronger
            _avoidanceForce = _avoidanceDirection * _proximityFactor * avoidanceWeight;
        }

        return _avoidanceForce;
    }

    public static Vector2 CalculateWanderForce(BirdBrain bird, float speedLimit, float steerForceLimit, float wanderRingDistance, float wanderRingRadius)
    {
        Vector2 _ringCenter = (Vector2)bird.transform.position + bird.RigidBody.velocity.normalized * wanderRingDistance;
        bird.TargetPosition = _ringCenter + wanderRingRadius * UnityEngine.Random.insideUnitCircle.normalized;
        return Seek(bird, speedLimit, steerForceLimit);
    }

    public static Vector2 CalculateBoidForce(BirdBrain bird, float maxFlockMates, float separationWeight, float alignmentWeight, float cohesionWeight)
    {
        Vector2 _separation = Vector2.zero; // Prevents birds getting too close
        Vector2 _alignment = Vector2.zero; // Urge to match direction of others
        Vector2 _cohesion = Vector2.zero; // Urge to move towards centroid of flock
        int _count = 0;

        var _nearbyBirds = bird.NearbyBirdTracker.NearbyBirds
            .Where(b => bird.FlockableBirdsNames.Contains(b.BirdName))
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
            return
                (_separation.normalized * separationWeight +
                _alignment.normalized * alignmentWeight +
                _cohesion * cohesionWeight).normalized;
        }
        return Vector2.zero;
    }

    public static Vector2 Seek(BirdBrain bird, float speedLimit, float steerForceLimit)
    {
        Vector2 _desired = (bird.TargetPosition - (Vector2)bird.transform.position).normalized * speedLimit;
        Vector2 _steer = _desired - bird.RigidBody.velocity;
        if (_steer.magnitude >= steerForceLimit)
            _steer = _steer.normalized * steerForceLimit;

        return _steer;
    }
}