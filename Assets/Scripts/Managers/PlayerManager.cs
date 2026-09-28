using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Host manages all player stuff.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    // Keep track of who is the host, and who is the local player
    public static Player LocalPlayer { get; private set; }
    public Transform[] spawnPoints;
    void Awake()
    {
        Instance = this;
    }
    public static Transform GetSpawnPoint(ulong index)
    {
        return Instance.spawnPoints[index];
    }

    public static Transform GetRandomSpawnPoint()
    {
        int count = Instance.spawnPoints.Length;
        return Instance.spawnPoints[Random.Range(0, count)];
    }
}
