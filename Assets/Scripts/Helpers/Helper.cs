using UnityEngine;

public static class Helper
{
    public static Vector3 RandomTorque(float minTorque, float maxTorque)
    {
        // Combine Random Direction and Random Torque
        return Random.onUnitSphere * Random.Range(minTorque, maxTorque);
    }
}