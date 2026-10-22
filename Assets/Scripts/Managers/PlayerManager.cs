using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Host manages all player stuff.
/// </summary>
[DefaultExecutionOrder(-100)] 
public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }
    
    #region Local Player Tracking
    // Keep track of who is the local player
    public static Player LocalPlayer { get; private set; }
    public static BuildCamera LocalBuilder {get; private set; }

    // The Player's hud. Only one instance of this should exist in the scene.
    [SerializeField] private PlayerHud playerHud;
    [SerializeField] private BuildingUI builderHud;
    public static BuildingUI BuildingUI => Instance.builderHud;

    // State Tracking
    public static PointOfView POV { get; private set; }

    // Client Tracking
    public static bool IsHostClient { get; private set; }
    public static ulong LocalClientId { get; private set; } // NetworkManager.Singleton.LocalClientId;
    public static NetworkClient LocalClient { get; private set; } // NetworkManager.Singleton.LocalClient;
    
    // Local Client Events
    public static event Action OnPlayerReset;
    public static event Action ToBuilding;
    public static event Action ToCombat;

    // Player Settings
    public static PlayerSettings PlayerSettings {get; private set;}
    [SerializeField] private PlayerSettings debugPlayerSettings;
    #endregion

    // Debugger
    private DebugConfig debugConfig;
    public static ConfigType ConfigType => Instance.debugConfig.debuggingType;

    // Spawnpoint transforms
    public Transform[] spawnPoints;
    
    // Spawnpoint tracker
    private readonly HashSet<int> takenSpawns = new();

    // Private Prefabs
    [SerializeField] private Player playerPrefab;
    [SerializeField] private BuildCamera builderPrefab;

    // Server-side events - Subscribe methods to these depending on what the config says
    public static event Action<ulong> OnPlayerDeath;   // What happens when the player dies.
    public static event Action OnAllPlayersReset;
    private static int DefaultPov;

    void Awake()
    {
        Instance = this;
        Debug.Log("PlayerManager is Awakened");


        // Dont rely on this, this is for debugging only and it is also temporary
        if (debugPlayerSettings != null) SetPlayerSettings(debugPlayerSettings);

    }
    /// <summary>
    /// Clean up stale RPC events we are subscribed to
    /// </summary>
    private void OnDestroy()
    {
        if (Instance != this) return;

        OnPlayerDeath = null;
        OnAllPlayersReset = null;
        OnPlayerReset = null;
        ToBuilding = null;
        ToCombat = null;

        LocalPlayer = null;
        LocalBuilder = null;
        LocalClient = null;

        Instance = null;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

    }

    /// <summary>
    /// Server-side Configuration
    /// </summary>
    /// <param name="config"></param>
    public void ConfigureSpawning(DebugConfig config)
    {
        debugConfig = config;
        // If it was configured, then run in debug mode
        // If not, carry on with normal game

        switch (debugConfig.debuggingType)
        {
            case ConfigType.Solo_Fps:
                DefaultPov = 0;

                // Host is controlling themselves so this is ok, but dont copy this for multiplayer
                NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
                NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
                {
                    ChangePov(DefaultPov);
                };
                // Host Respawn the player immediately (the host is respawning themselves so its ok)
                OnPlayerDeath += (clientId) =>
                {
                    Invoke(nameof(ClearSpawnPoints), 1f);
                    Invoke(nameof(ServerPlayersReset), 1f);
                    Invoke(nameof(ResetPlayer), 1f);
                    Invoke(nameof(ToDefaultPov), 1f);
                };
            break;

            case ConfigType.Solo_Building:
                DefaultPov = 1;

                NetworkManager.Singleton.OnClientConnectedCallback += SpawnBuilder;
                NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
                {
                    ChangePov(1);
                };
            break;

            case ConfigType.Full_Game:
                DefaultPov = 3; // 3 means there is no default pov, because the players will be swapping between player, builder, and spectator

                NetworkManager.Singleton.OnClientConnectedCallback += SpawnBuilder;
                NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;

                // Everything below is to set up the Server-side PlayerManager
                // Tell the gamemanager that the player is spawned and loaded.
                NetworkManager.Singleton.OnClientConnectedCallback += GameManager.Instance.PlayerLoaded;

                // The host will reset server-side players
                OnAllPlayersReset += ClearSpawnPoints;
                OnAllPlayersReset += ServerPlayersReset;

                // The host/server will tell all clients to reset their players
                OnAllPlayersReset += ResetAllPlayers;

                // Tell gamemanager that a player had died
                OnPlayerDeath += GameManager.Instance.PlayerDeath;

            break;

            case ConfigType.Single_Player_Full:
                DefaultPov = 1; // Default POV is builder

                NetworkManager.Singleton.OnClientConnectedCallback += SpawnBuilder;
                NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
                
                NetworkManager.Singleton.OnClientConnectedCallback += GameManager.Instance.PlayerLoaded;

                NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
                {
                    ChangePov(1);
                };

                // Host Respawn the player immediately (the host is respawning themselves so its ok)
                OnPlayerDeath += (clientId) =>
                {
                    Invoke(nameof(ClearSpawnPoints), 2f);
                    Invoke(nameof(ServerPlayersReset), 2f);
                    Invoke(nameof(ClientPlayerResetRpc), 2f);
                };

            break;
        }
    }

    /// <summary>
    /// fix for certain events not being registered when entering from main menu
    /// </summary>
    public void ConfigureGameplayEvents()
    {
        if (!IsServer) return;

        OnAllPlayersReset -= ClearSpawnPoints;
        OnAllPlayersReset -= ServerPlayersReset;
        OnAllPlayersReset -= ResetAllPlayers;
        OnPlayerDeath -= GameManager.Instance.PlayerDeath;

        OnAllPlayersReset += ClearSpawnPoints;
        OnAllPlayersReset += ServerPlayersReset;
        OnAllPlayersReset += ResetAllPlayers;
        OnPlayerDeath += GameManager.Instance.PlayerDeath;
    }

    /// <summary>
    /// Called by Server
    /// This method invokes an event in which a game-wide reset will occur
    /// </summary>
    public static void ResetEvent()
    {
        OnAllPlayersReset?.Invoke();
    }

    /// <summary>
    /// Invoked by OnAllPlayersReset on Server
    /// The Server will tell all players to reset themselves on their local clients
    /// </summary>
    private void ResetAllPlayers()
    {
        ClientPlayerResetRpc();
    }

    
    /// <summary>
    /// Called by Server
    /// Server will reset network variables for each player
    /// </summary>
    private void ServerPlayersReset()
    {
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Player player = client.PlayerObject.GetComponent<Player>();
            player.health.Value = 100;
            player.UpdateHealthClientRpc(player.OwnerClientId);
            player.isAlive.Value = true;
            player.Combat.EmptyWeapon();
            player.Combat.weaponHandler.DropWeapon();
        }
    }

    /// <summary>
    /// Set the player settings
    /// </summary>
    /// <param name="newPlayerSettings"></param>
    public void SetPlayerSettings(PlayerSettings newPlayerSettings)
    {
        Debug.Log("Player Settings Set");
        PlayerSettings = newPlayerSettings;
    }
    /// <summary>
    /// Invoked by server. Method is subscribed to server's OnAllPlayersReset.
    /// The host will tell all clients to reset their local players.
    /// </summary>
    [Rpc(SendTo.Everyone)]
    private void ClientPlayerResetRpc()
    {
        ResetPlayer();
        ResetNonLocalPlayers();

        OnPlayerReset?.Invoke();
    }

#region Player Setup
    /// <summary>
    /// Called while on the server
    /// Spawns a player for a client and sets its playerobject
    /// </summary>
    /// <param name="clientId"></param>
    public void SpawnPlayer(ulong clientId)
    {
        if (!IsServer) return;
        Transform spawnPoint = GetRandomSpawnPoint();
        Vector3 pos = spawnPoint.position; pos.y += 1.5f;
        Player player = Instantiate(
            playerPrefab,
            pos,
            spawnPoint.rotation
        );

        player.NetworkObject.SpawnAsPlayerObject(clientId, true);
        Debug.Log("Player Spawned for Client: " + clientId);
        // Set up player Serverside
        // Add events that would actually effect the server when the player dies.
        player.AddDeathEvent(() =>
        {
            OnPlayerDeath?.Invoke(clientId);
        });
    }

    /// <summary>
    /// Called by Client Player Script
    /// Set the Local Player for the local PlayerManager
    /// </summary>
    /// <param name="p"></param>
    public void SetLocalPlayer(Player p)
    {
        // Set up player local-side
        IsHostClient = IsHost;
        LocalClientId = NetworkManager.Singleton.LocalClientId;
        LocalClient = NetworkManager.Singleton.LocalClient;
        LocalPlayer = p;
        Debug.Log("Local Player is: " + LocalClientId);
        // Set up hud
        LocalPlayer.SetHud(playerHud);
        playerHud.health.text = LocalPlayer.MaxHealth.ToString();
        LocalPlayer.Look.SetDefaultFOV(PlayerSettings.DefaultPOV);
        OnPlayerReset += OnReset;

        if (LocalBuilder) ChangePov(1);
    }


    /// <summary>
    /// Called by Client
    /// Local clients need to reset their versions of other players
    /// </summary>
    private void ResetNonLocalPlayers()
    {
        foreach (NetworkClient networkClient in NetworkManager.Singleton.ConnectedClientsList)
        {
            Player player = networkClient.PlayerObject.GetComponent<Player>();

            if (player == null || player == LocalPlayer)
                continue;

            player.Body.UnRagdoll();
            player.Body.Play("IsMoving", false);
            player.Move.OnDeathCollider(false);
            Destroy(player.Combat.weaponInHand);
        }
    }
    
    /// <summary>
    /// Called by Client
    /// Tell the Server's GameManager that this player has reset
    /// </summary>
    private void OnReset()
    {
        if(GameManager.Instance)
            GameManager.Instance.PlayerResetServerRpc();
    }
#endregion

#region Builder Setup

    /// <summary>
    /// Called by Server
    /// Spawns a builder for a client and sets its ownership to the client
    /// </summary>
    /// <param name="clientId"></param>
    public void SpawnBuilder(ulong clientId)
    {
        if (!IsServer) return;
        BuildCamera buildCam = Instantiate(
            builderPrefab
        );

        buildCam.NetworkObject.SpawnWithOwnership(clientId, true);
    }

    /// <summary>
    /// Called by Local Client BuilderCamera
    /// Set the Local Builder for the local PlayerManager
    /// </summary>
    /// <param name="cam"></param>
    public void SetLocalBuilder(BuildCamera cam)
    {
        if (!LocalPlayer)
        {
            IsHostClient = IsHost;
            LocalClientId = NetworkManager.Singleton.LocalClientId;
            LocalClient = NetworkManager.Singleton.LocalClient;
        }
        // Get the builder object from the NetworkManager's SpawnManager's SpawnedObjects dictionary.
        Debug.Log("Local Builder Is: " + LocalClientId);
        
        LocalBuilder = cam;
        SetUpBuilder();
    }

    /// <summary>
    /// Called by Client
    /// Subscribe methods to OnPlayerReset from the LocalBuilder and builderHud 
    /// </summary>
    private void SetUpBuilder()
    {
        // The manager is the one who handles the resetting stuff.
        OnPlayerReset += LocalBuilder.gridBuilding.ResetCounter;
        OnPlayerReset += builderHud.GenerateRandomChoices;
    }


#endregion

#region Player Managing

    /// <summary>
    /// Needs to be called by Client or ClientRpc
    /// Changes the POV and controls of LOCAL Client
    /// </summary>
    /// <param name="newPov"></param>
    public void ChangePov(int newPov)
    {
        POV = (PointOfView) newPov;

        switch (POV)
        {
            case PointOfView.FPS:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                
                if (LocalPlayer)
                { 
                    LocalPlayer.Look.cam.gameObject.SetActive(true);
                    LocalPlayer.Body.ShowBodyRenderer(false);
                    LocalPlayer.deathCamera.Toggle(false);
                }
                if (LocalBuilder) LocalBuilder.gameObject.SetActive(false);

                if (builderHud) builderHud.gameObject.SetActive(false);  
                if (playerHud) playerHud.gameObject.SetActive(true);                 
                TogglePlayerControls(true);

                ToCombat?.Invoke();
            break;
            case PointOfView.Building:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;

                if (LocalPlayer)
                {
                    LocalPlayer.Look.cam.gameObject.SetActive(false);
                    LocalPlayer.Body.ShowBodyRenderer(true);
                    LocalPlayer.deathCamera.Toggle(false);
                } 
                if (LocalBuilder) LocalBuilder.gameObject.SetActive(true);
                if (builderHud) builderHud.gameObject.SetActive(true);  
                if (playerHud) playerHud.gameObject.SetActive(false);  

                TogglePlayerControls(false);
                ToBuilding?.Invoke();
            break;
            case PointOfView.Spectate:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;

                if (LocalPlayer) LocalPlayer.Look.cam.gameObject.SetActive(false);
                if (LocalBuilder) LocalBuilder.gameObject.SetActive(false);

                // Spectate Cam needs to be added soon.
                TogglePlayerControls(false);
            break;
        }
    }

    /// <summary>
    /// This is for debugging. It forces the pov to be the default. DO NOT use this for the actual game.
    /// </summary>
    private void ToDefaultPov()
    {
        if (DefaultPov == -1) return;
        ChangePov(DefaultPov);
    }
    /// <summary>
    /// Swaps the LOCAL Player Controls.
    /// </summary>
    /// <param name="canMove">True = FPS Input is on. False = Builder Input is on</param>
    public static void TogglePlayerControls(bool canMove)
    {
        if (LocalPlayer) LocalPlayer.ToggleMove(canMove);
        if (LocalBuilder) LocalBuilder.ToggleInput(!canMove);
    }

    /// <summary>
    /// Disables control in both modes for when the exit menu panel is open
    /// </summary>
    public static void DisableAllPlayerControls()
    {
        if (LocalPlayer) LocalPlayer.ToggleMove(false);
        if (LocalBuilder) LocalBuilder.ToggleInput(false);
    }


    /// <summary>
    /// Needs to be called by Client or ClientRpc
    /// </summary>
    private void ResetPlayer()
    {
        TeleportPlayer(GetRandomSpawnPoint().position);
        LocalPlayer.Revive();
    }

    private void TeleportPlayer(Vector3 pos)
    {
        LocalPlayer.transform.position = pos;
    }

#endregion

// Might just move this into its own class
#region Spawnpoint Management
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


    public void ClearSpawnPoints()
    {
        Instance.takenSpawns.Clear();
    }

#endregion

}

public enum PointOfView
{
    FPS = 0,
    Building = 1,
    Spectate = 2,
}
