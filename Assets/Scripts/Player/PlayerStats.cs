using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> PlayerName = new(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> Kills = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> Deaths = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> RoundsWon = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (Scoreboard.Instance != null)
        {
            Scoreboard.Instance.RegisterPlayer(this);
        }

        if (IsOwner)
        {
            string playerName = "No Name";
            if (LobbyManager.Instance)
            {                
                playerName =
                    LobbyManager.Instance.CurrentSession.CurrentPlayer.GetPlayerName();
            }
            else
            {
                if (IsServer)
                    playerName = "(Host) Player " + OwnerClientId;
                else
                    playerName = "Player " + OwnerClientId;
            }
                
            
            
            SetPlayerNameServerRpc(playerName);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (Scoreboard.Instance != null)
        {
            Scoreboard.Instance.UnregisterPlayer(this);
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerNameServerRpc(string playerName)
    {
        PlayerName.Value = playerName;
    }

    public void AddKill()
    {
        if (!IsServer) return;

        Kills.Value++;
    }

    public void AddDeath()
    {
        if (!IsServer) return;

        Deaths.Value++;
    }

    public void AddRoundWin()
    {
        if (!IsServer) return;

        RoundsWon.Value++;
    }
}