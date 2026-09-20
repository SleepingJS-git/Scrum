using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;
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
