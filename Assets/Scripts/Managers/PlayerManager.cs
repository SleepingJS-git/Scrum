using System;
using System.Collections;
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
        return Instance.spawnPoints[UnityEngine.Random.Range(0, count)];
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
