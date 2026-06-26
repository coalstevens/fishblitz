using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IFlockMate
{
    Vector2 Position { get; }
    Vector2 Velocity { get; }
}

public static class SteeringForces
{
    public static Vector2 Seek(Vector2 position, Vector2 velocity, Vector2 targetPosition, float speedLimit, float steerForceLimit)
    {
        Vector2 desired = (targetPosition - position).normalized * speedLimit;
        Vector2 steer = desired - velocity;
        if (steer.magnitude >= steerForceLimit)
            steer = steer.normalized * steerForceLimit;

        return steer;
    }

    public static Vector2 CalculateWanderForce(
        Vector2 position,
        Vector2 velocity,
        float speedLimit,
        float steerForceLimit,
        float wanderRingDistance,
        float wanderRingRadius,
        ref Vector2 targetPosition,
        out Vector2 ringCenter)
    {
        ringCenter = position + velocity.normalized * wanderRingDistance;
        targetPosition = ringCenter + wanderRingRadius * Random.insideUnitCircle.normalized;
        return Seek(position, velocity, targetPosition, speedLimit, steerForceLimit);
    }

    public static Vector2 CalculateAvoidanceForce(
        Vector2 position,
        Vector2 velocity,
        float circleCastRadius,
        float circleCastRange,
        float avoidanceWeight,
        LayerMask interactionLayers,
        out Vector2 obstaclePosition)
    {
        obstaclePosition = Vector2.zero;

        if (circleCastRange <= 0 || circleCastRadius <= 0)
        {
            Debug.LogWarning("CircleCast parameters should be greater than zero.");
            return Vector2.zero;
        }

        RaycastHit2D hit = Physics2D.CircleCast(
            position,
            circleCastRadius,
            velocity.normalized,
            circleCastRange,
            interactionLayers
        );

        if (hit && !hit.collider.isTrigger)
        {
            float proximityFactor = 1 - hit.fraction;
            Vector2 avoidanceForce = proximityFactor * avoidanceWeight * hit.normal;
            obstaclePosition = hit.point;
            return avoidanceForce;
        }

        return Vector2.zero;
    }

    public static Vector2 CalculateBoidForce<T>(
        Vector2 position,
        Vector2 velocity,
        IEnumerable<T> flockMates,
        float boidForceWeight,
        float maxMates,
        float separationWeight,
        float alignmentWeight,
        float cohesionWeight) where T : IFlockMate
    {
        Vector2 separation = Vector2.zero;
        Vector2 alignment = Vector2.zero;
        Vector2 cohesion = Vector2.zero;
        int count = 0;

        var sortedMates = flockMates
            .Where(m => m != null)
            .OrderBy(m => Vector2.Distance(position, m.Position));

        foreach (var mate in sortedMates)
        {
            float distance = Vector2.Distance(position, mate.Position);
            separation += (position - mate.Position) / distance;
            alignment += mate.Velocity;
            cohesion += mate.Position;
            count++;

            if (count >= maxMates)
                break;
        }

        if (count > 0)
        {
            separation /= count;
            alignment /= count;
            cohesion /= count;
            cohesion = (cohesion - position).normalized;
            Vector2 boidForce =
                (separation.normalized * separationWeight +
                alignment.normalized * alignmentWeight +
                cohesion * cohesionWeight).normalized;
            return boidForce * boidForceWeight;
        }

        return Vector2.zero;
    }

    public static LayerMask GetInteractionLayers(GameObject gameObject, Rigidbody2D rb)
    {
        int objLayer = gameObject.layer;
        int mask = 0;

        for (int i = 0; i < 32; i++)
            if (!Physics2D.GetIgnoreLayerCollision(objLayer, i))
                mask |= (1 << i);

        mask &= ~(1 << objLayer);

        if (rb != null)
        {
            mask &= ~(rb.excludeLayers);
            mask |= rb.includeLayers;
        }

        return mask;
    }
}
