using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public BuildingUI buildingUI;
    public ConnectUI connectUI;
    [SerializeField] private GamePhase gamePhase;
    public static GamePhase GamePhase => Instance.gamePhase;
    [SerializeField] private int numOfPlayers;
    private int _loadedPlayers;
    private int _deadPlayers;
    private int _readyPlayers;
    private int _resetPlayers;
    public static bool CanMove => 
        Instance.gamePhase == GamePhase.Combat || 
        Instance.gamePhase == GamePhase.EndOfCombat;
    void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        // Test, eventually there needs to be code to check if all
        // players spawned in.

        if (!IsServer) return;
        ChangeGamePhaseServerRpc(GamePhase.Loading);


    }

    /// <summary>
    /// When the players get loaded, set the number of players that the manager will wait to load.
    /// Once every player is loaded, game will begin.
    /// </summary>
    [Rpc(SendTo.Server)]
    public void SetNumOfPlayersServerRpc(int num)
    {
        numOfPlayers = num;
    }

    [Rpc(SendTo.Server)]
    public void PlayerLoadedServerRpc(RpcParams rpcParams = default)
    {
        _loadedPlayers++;

        Debug.Log($"Players Joined: {_loadedPlayers} / {numOfPlayers}");
    }

    public void PlayerDeath(ulong clientID)
    {

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientID, out NetworkClient client))
        {
            Player player = client.PlayerObject.GetComponent<Player>();

            _deadPlayers++;

            Debug.Log("Dead Players: " + _deadPlayers);
        }
    }

    [Rpc(SendTo.Server)]
    public void PlayerResetServerRpc()
    {
        _resetPlayers++;

        Debug.Log($"Players Reset: {_resetPlayers} / {numOfPlayers}");
    }

    [Rpc(SendTo.Server)]
    public void PlayerReadyServerRpc()
    {
        _readyPlayers++;

        Debug.Log($"Players Ready: {_loadedPlayers} / {numOfPlayers}");
    }


    void Update()
    {
        if (!IsServer) return;

        switch (gamePhase)
        {
            case GamePhase.Loading:
                if (_loadedPlayers == numOfPlayers)
                {
                    if (gamePhase != GamePhase.Building)
                        ChangeGamePhaseServerRpc(GamePhase.Building);
                }
                break;
            case GamePhase.Building:
                if (_readyPlayers == numOfPlayers)
                {
                    if (gamePhase != GamePhase.Combat)
                        ChangeGamePhaseServerRpc(GamePhase.Combat);
                }
                break;
            case GamePhase.Combat:
                if (_deadPlayers >= numOfPlayers - 1)
                {
                    if (gamePhase != GamePhase.EndOfCombat)
                        ChangeGamePhaseServerRpc(GamePhase.EndOfCombat);
                }
                break;
            case GamePhase.EndOfCombat:
                if (_resetPlayers == numOfPlayers)
                {
                    if (gamePhase != GamePhase.Building)
                        ChangeGamePhaseServerRpc(GamePhase.Building);
                }
                break;
        }
    }


    public void ChangeToCombat()
    {
        ChangeGamePhaseServerRpc(GamePhase.Combat);
    }
    public void ChangeToBuilding()
    {
        ChangeGamePhaseServerRpc(GamePhase.Building);
    }
    [Rpc(SendTo.Server)]
    private void ChangeGamePhaseServerRpc(GamePhase gp)
    {
        gamePhase = gp;
        switch (gamePhase)
        {
            case GamePhase.Combat:
                _readyPlayers = 0;
                _resetPlayers = 0;
                _deadPlayers = 0;
                CamControlClientRpc(true, (int)gamePhase);
                break;

            case GamePhase.Building:
                _readyPlayers = 0;
                CamControlClientRpc(false, (int)gamePhase);
                break;

            case GamePhase.EndOfCombat:
                _resetPlayers = 0;
                Invoke(nameof(ReviveAllPlayers), 2f);
                break;
        }
    }
    /// <summary>
    /// Revive all Players
    /// </summary>
    private void ReviveAllPlayers()
    {
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Player player = client.PlayerObject.GetComponent<Player>();
            player.health.Value = 100;
            player.isAlive.Value = true;
            player.buildCam.gridBuilding.currentPlacements.Value = 0;
        }

        EndCombatClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EndCombatClientRpc()
    {
        foreach (NetworkObject networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            Player player = networkObject.GetComponent<Player>();

            if (player == null)
                continue;

            if (player.IsOwner)
            {
                player.combat.EmptyWeapon();
                player.ToggleDeathHud(false);
                player.body.UnRagdoll(false);
                player.body.Play("IsMoving", false);
                player.SpawnServerRpc();
                player.PlayerIsReset();
            }
            else
            {
                Destroy(player.combat.weaponInHand);
                player.body.UnRagdoll(true);
            }

            player.move.OnDeathCollider(false);
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void CamControlClientRpc(bool toFps, int gamePhaseInt)
    {
        gamePhase = (GamePhase)gamePhaseInt;

        if (gamePhase == GamePhase.Building) connectUI.ResetReadyButton();
        buildingUI.gameObject.SetActive(!toFps);

        Player player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();

        player.ToggleFirstPerson(toFps);
    }

}

public enum GamePhase
{
    Loading,
    Combat,
    EndOfCombat,
    Building
}
