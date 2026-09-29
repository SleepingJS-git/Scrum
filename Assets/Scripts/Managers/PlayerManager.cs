using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Host manages all player stuff.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    // Keep track of who is the local player
    public static Player LocalPlayer { get; private set; }
    public static BuildCamera LocalBuilder {get; private set; }

    // The Player's hud. Only one instance of this should exist in the scene.
    [SerializeField] private PlayerHud playerHud;
    public PlayerHud PlayerHud => playerHud;

    // Player Events | ulong is clientId
    public static event Action<ulong> OnPlayerSpawned;
    public static event Action<ulong> OnPlayerDeath;
    public static event Action<ulong> OnPlayerReset;

    // Debugger
    public static DebugManager DebugManager {get; private set;} 

    public static bool IsHost {get; private set;}  // NetworkManager.Singleton.IsHost;
    public static ulong LocalClientId {get; private set;} // NetworkManager.Singleton.LocalClientId;
    public static NetworkClient LocalClient {get; private set;} // NetworkManager.Singleton.LocalClient;

    public Transform[] spawnPoints;

    private readonly HashSet<int> takenSpawns = new();
    void Awake()
    {
        Instance = this;
        Debug.Log("PlayerManager is Awakened");

        // Check if the LobbyManager was created. If yes, then the game was loaded from a Lobby. 
        // If not, the game is being ran in Play Mode directly in the scene.
        if (LobbyManager.Instance)
        {
            // If this is the normal game, then we can get stuff from LobbyManager to set up all players.
            // Im pretty sure NetworkPlayerSpawner is also only created when loading from lobby.
        }
        else
        {
            DebugManager = GetComponentInChildren<DebugManager>();
            if (DebugManager) ConfigureDebugManager();
        }

        OnPlayerSpawned += SetLocalPlayer;

        // Wait for NetworkManager to load
        StartCoroutine(WaitForNetworkManager());
    }
    private IEnumerator WaitForNetworkManager()
    {
        while (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening)
        {
            yield return null;
        }

        // Once the NetworkManager is ready to go, now try to spawn player based on LocalClient
        OnPlayerSpawned?.Invoke(NetworkManager.Singleton.LocalClientId);
    }
    private void SetLocalPlayer(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            LocalClientId = clientId;
            LocalClient = client;
            LocalPlayer = LocalClient.PlayerObject.GetComponent<Player>();
        }
    }
    public static Transform GetSpawnPoint(ulong index)
    {
        return Instance.spawnPoints[index];
    }

    public static Transform GetRandomSpawnPoint()
    {
        int count = Instance.spawnPoints.Length;

        List<int> availableSpawns = new();

        //find all the available spawn points
        for (int i = 0; i < count; i++)
        {
            if (!Instance.takenSpawns.Contains(i))
                availableSpawns.Add(i);
        }

        //all spawn points taken
        if (availableSpawns.Count == 0)
        {
            Debug.LogWarning("No available spawn points!");
            return Instance.spawnPoints[UnityEngine.Random.Range(0, count)];
        }

        int chosenIndex = availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Count)];
        Instance.takenSpawns.Add(chosenIndex);
        return Instance.spawnPoints[chosenIndex];
    }

    public static void ClearSpawnPoints()
    {
        Instance.takenSpawns.Clear();
    }

    private void ConfigureDebugManager()
    {
        DebugManager.enabled = true;
        
        DebugConfig config = DebugManager.debugConfig;

        if (!config.configured)
        {
            config = new DebugConfig();
            // Depending on the scene, we have to register the player differently.
            switch (SceneManager.GetActiveScene().name)
            {
                case "Player Testing (Single Player)":
                    config.debuggingType = DebugType.Fps;
                    
                break;

                case "MVP Testing":
                    config.debuggingType = DebugType.MVP;
                break;
            }
        }

        DebugManager.ConfigureDebugManager(config);
    }

}
