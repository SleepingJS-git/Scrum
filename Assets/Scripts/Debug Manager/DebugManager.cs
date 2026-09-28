using System;
using Unity.Netcode;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public DebugConfig debugConfig;
    [SerializeField] private Player playerPrefab;
    void Start()
    {
        switch (debugConfig.debuggingType)
        {
            case DebugType.Solo_Fps:
            case DebugType.Solo_Building:
                NetworkManager.Singleton.StartHost();
            break;

        }
        
    }

    /// <summary>
    /// Does nothing yet.
    /// </summary>
    /// <param name="config"></param>
    public void ConfigureDebugManager(DebugConfig config)
    {
        debugConfig = config;
        switch (debugConfig.debuggingType)
        {
            case DebugType.Fps:
                config.OnPlayerSpawned += FPSOnly;
            break;
            case DebugType.Solo_Building:
                
            break;

        }
    }

    private void FPSOnly(ulong clientId)
    {
        Transform spawnPoint = PlayerManager.GetRandomSpawnPoint();
        Player player = Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        player.NetworkObject.SpawnAsPlayerObject(clientId, true);
    }
}

[Serializable]
public class DebugConfig
{
    public bool configured;
    public DebugType debuggingType;
    public Action<ulong> OnPlayerSpawned;

    public DebugConfig()
    {
        configured = false;
    }
}

public enum DebugType
{
    Solo_Fps,
    Solo_Building,
    Networked_Fps,
    Networked_Building, // Add more in the future
    MVP,
    Building,
    Fps
}
