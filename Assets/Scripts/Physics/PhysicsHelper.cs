using System.Collections;
using UnityEngine;

/// <summary>
/// This class is to help do physics math for objects that do not have rigid bodies, like the Player.
/// </summary>
public static class PhysicsHelper
{
    /// <summary>
    /// Immediate velocity change.
    /// </summary>
    /// <param name="dir">Direction of force</param>
    /// <param name="force">Force of the impact</param>
    /// <param name="mass">Mass of object affected</param>
    /// <returns>(Vector3) Resulting velocity from incoming force</returns>
    public static Vector3 Impulse(Vector3 dir, float force, float mass)
    {
        return dir * (force / mass);
    }

    /// <summary>
    /// The force decreases linearly with distance. A target at the center receives the maximum force, while a target at the radius boundary receives zero.
    /// </summary>
    /// <param name="explosionPos">Position of the explosion</param>
    /// <param name="targetPos">Position of the affected target</param>
    /// <param name="explosionForce">Force of the explosion</param>
    /// <param name="explosionRadius">Radius of the explosion</param>
    /// <param name="upwardsModifier">Upwards modifier</param>
    /// <returns>(Vector3) Resulting direction and magnitude</returns>
    public static Vector3 ExplosionForce(
        Vector3 explosionPos, 
        Vector3 targetPos, 
        float explosionForce, 
        float explosionRadius, 
        float upwardsModifier)
    {
        Vector3 dir = targetPos - explosionPos; // direction away from explosion
        float dist = dir.magnitude;

        dir.y += upwardsModifier;

        if (dist > explosionRadius) return Vector3.zero;

        // Get a distance multiplier
        float distMult = 1f - (dist / explosionRadius);

        return distMult * explosionForce * dir.normalized;
    }
}