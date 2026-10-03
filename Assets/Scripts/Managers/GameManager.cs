using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using TMPro;
using System;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public ConnectUI connectUI;
    [SerializeField] private GamePhase gamePhase;
    public static GamePhase GamePhase => Instance.gamePhase;
    [SerializeField] private int numOfPlayers;
    [SerializeField] private TMP_Text roundWinnerText;
    [SerializeField] private CanvasGroup roundWinnerCanvasGroup;
    private int _loadedPlayers;
    private int _deadPlayers;
    private int _readyPlayers;
    private int _resetPlayers;
    public NetworkVariable<FixedString512Bytes> debugInfo = new(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
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
    /// Set player count, used in NetworkSpawner after spawning players in
    /// </summary>
    /// <param name="count"></param>
    public void SetPlayerCount(int count)
    {
        numOfPlayers = count;
    }
    
    /// <summary>
    /// Called on Server.
    /// Player is loaded
    /// </summary>
    /// <param name="clientId"></param>
    public void PlayerLoaded(ulong clientId)
    {
        if (!IsServer) return;
        _loadedPlayers++;

        Debug.Log($"Players Joined: {_loadedPlayers} / {numOfPlayers}");

        if (_loadedPlayers == numOfPlayers)
        {
            ChangeGamePhaseServerRpc(GamePhase.Building);
        }
    }

    /// <summary>
    /// Called on server.
    /// This player has died.
    /// </summary>
    /// <param name="clientID"></param>
    public void PlayerDeath(ulong clientID)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientID, out NetworkClient client))
        {
            Player player = client.PlayerObject.GetComponent<Player>();

            if (player.isAlive.Value) return;   // Player is not dead.
            PlayerStats stats = client.PlayerObject.GetComponent<PlayerStats>();
            stats.AddDeath();


            _deadPlayers++;

            Debug.Log("Dead Players: " + _deadPlayers);
        }

        if (_deadPlayers >= numOfPlayers - 1)
        {
            ChangeGamePhaseServerRpc(GamePhase.EndOfCombat);
        }

    }

    [Rpc(SendTo.Server)]
    public void PlayerResetServerRpc()
    {
        _resetPlayers++;

        Debug.Log($"Players Reset: {_resetPlayers} / {numOfPlayers}");

        if (_resetPlayers == numOfPlayers)
        {
            if (gamePhase != GamePhase.Building)
                ChangeGamePhaseServerRpc(GamePhase.Building);
        }
    }

    [Rpc(SendTo.Server)]
    public void PlayerReadyServerRpc()
    {
        _readyPlayers++;

        Debug.Log($"Players Ready: {_loadedPlayers} / {numOfPlayers}");

        if (_readyPlayers == numOfPlayers)
            ChangeGamePhaseServerRpc(GamePhase.Combat);
    }
    /// <summary>
    /// Debug Button
    /// </summary>
    public void ChangeToCombat()
    {
        ChangeGamePhaseServerRpc(GamePhase.Combat);
    }

    /// <summary>
    /// Debug Button
    /// </summary>
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
                PlayerControllerRpc((int) gamePhase);
                break;

            case GamePhase.Building:
                _readyPlayers = 0;
                _resetPlayers = 0;
                _deadPlayers = 0;
                PlayerControllerRpc((int) gamePhase);
                break;

            case GamePhase.EndOfCombat:
                _readyPlayers = 0;
                _resetPlayers = 0;
                _deadPlayers = 0;
                ShowRoundWinner();
                Invoke(nameof(ResetPlayers), 5f);
                break;
        }
    }

    private void ResetPlayers()
    {
        PlayerManager.ResetEvent();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerControllerRpc(int gamePhaseInt)
    {
        gamePhase = (GamePhase) gamePhaseInt;

        if (gamePhase == GamePhase.Building)
        {
            PlayerManager.ChangePov(1);
            roundWinnerCanvasGroup.alpha = 0f;
            connectUI.ResetReadyButton();
        }
        else if (gamePhase == GamePhase.Combat)
        {
            PlayerManager.ChangePov(0);
        }
    }


    public FixedString512Bytes DebugInfo()
    {
        //DebugInfoServerRpc();
        return debugInfo.Value;
    }

    //[Rpc(SendTo.Server)]
    //private void DebugInfoServerRpc()
    //{
    //    debugInfo.Value = $@"

    //    (Game Info)
    //    Game Phase: {GamePhase}
    //    Loaded Players: {_loadedPlayers} / {numOfPlayers}
    //    Ready Players: {_readyPlayers} / {numOfPlayers}
    //    Dead Players: {_deadPlayers} / {numOfPlayers}
    //    Revived Players: {_resetPlayers} / {numOfPlayers}
    //    ";
    //}

    /// <summary>
    /// Method for showing text declaring who won the round
    /// </summary>
    private void ShowRoundWinner()
    {
        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Player player = client.PlayerObject.GetComponent<Player>();

            if (player != null && player.isAlive.Value)
            {
                PlayerStats stats = client.PlayerObject.GetComponent<PlayerStats>();

                stats.AddRoundWin();

                string winnerName = stats.PlayerName.Value.ToString();

                ShowRoundWinnerRpc(winnerName);

                return;
            }
        }

        Debug.LogWarning("Could not find a surviving player.");
    }

    /// <summary>
    /// Update the round winner text on each player's screen
    /// </summary>
    /// <param name="winnerName"></param>
    [Rpc(SendTo.ClientsAndHost)]
    private void ShowRoundWinnerRpc(string winnerName)
    {
        roundWinnerText.text = $"{winnerName} won the round!";
        roundWinnerCanvasGroup.alpha = 1f;
    }

}



public enum GamePhase
{
    Loading,
    Combat,
    EndOfCombat,
    Building
}
