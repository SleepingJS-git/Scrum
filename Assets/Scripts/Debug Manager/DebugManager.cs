using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    private DebugConfig debugConfig;
    void Awake()
    {
        Instance = this;

        Debug.Log("DebugManager is Awakened");

        // If the network has not been created, create a host, then it will be.
        //if (NetworkManager.Singleton.IsListening) return;
        
        // Check if the LobbyManager was created. If yes, then the game was loaded from a Lobby. 
        // If not, the game is being ran in Play Mode directly in the scene.
        if (LobbyManager.Instance)
        {
            // If this is the normal game, then we can get stuff from LobbyManager to set up all players.
            // Im pretty sure NetworkPlayerSpawner is also only created when loading from lobby.
        }
        else
        {
            debugConfig = new DebugConfig();
            ConfigureDebugManager();
        }

    }

    void Start()
    {
        if (debugConfig == null) return;
        if (!debugConfig.IsConfigured)
        {
            NetworkManager.Singleton.StartClient();
            return;
        }
        switch (debugConfig.debuggingType)
        {
            case DebugType.Solo_Fps:
                NetworkManager.Singleton.StartHost();
                break;

            case DebugType.Solo_Building:
                NetworkManager.Singleton.StartHost();
                break;

            case DebugType.MVP:
                hostButton.gameObject.SetActive(true);
                clientButton.gameObject.SetActive(true);
                hostButton.onClick.AddListener(StartHost);
                clientButton.onClick.AddListener(StartClient);
                break;
        }

        // PlayerManager.Instance.GetComponent<NetworkObject>().Spawn();
    }

    private void StartHost()
    {
        PlayerManager.Instance.ConfigureSpawning(debugConfig);

        NetworkManager.Singleton.StartHost();
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    /// <summary>
    /// Does nothing yet.
    /// </summary>
    /// <param name="config"></param>
    private void ConfigureDebugManager()
    {   
        // Set up debug config by determining what scene we are in.
        switch (SceneManager.GetActiveScene().name)
        {
            case "Player Testing (Single Player)":
                debugConfig.debuggingType = DebugType.Solo_Fps;
                PlayerManager.Instance.ConfigureSpawning(debugConfig);

            break;

            case "GridBuilding (Single Player)":
                debugConfig.debuggingType = DebugType.Solo_Building;
                PlayerManager.Instance.ConfigureSpawning(debugConfig);

            break;

            case "MVP Testing (With New PlayerManager)":
                debugConfig.debuggingType = DebugType.MVP;
            break;
        }

        debugConfig.FinishConfiguration();
    }
}

[Serializable]
public class DebugConfig
{
    public bool IsConfigured{ get; private set; }
    public DebugType debuggingType;
    public bool IsHost;
    public DebugConfig()
    {
        IsConfigured = false;
    }

    public void FinishConfiguration()
    {
        IsConfigured = true;   
    }
}

public enum DebugType
{
    Solo_Fps,
    Solo_Building,
    Networked_Fps,
    Networked_Building, // Add more in the future
    MVP,
}
