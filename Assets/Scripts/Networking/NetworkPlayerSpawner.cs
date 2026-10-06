using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkPlayerSpawner : NetworkBehaviour
{
    // Make sure the host is the only one giving player spawns
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;

        Debug.Log("NetworkSpawner is Awakened");
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
            return;

        if (NetworkManager != null)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    // Spawn players once NGO has loaded the gameplay scene
    private void OnLoadEventCompleted(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (sceneName != gameObject.scene.name)
            return;

        foreach (ulong clientId in clientsCompleted)
        {
            // Give this client their FPS Player
            if (NetworkManager.ConnectedClients[clientId].PlayerObject == null)
            {
                PlayerManager.Instance.SpawnPlayer(clientId);
            }

            // Give this client their Builder
            PlayerManager.Instance.SpawnBuilder(clientId);

            // Tell GameManager this player finished loading
            GameManager.Instance.PlayerLoaded(clientId);
        }

        // Tell GameManager how many players are here
        GameManager.Instance.SetPlayerCount(clientsCompleted.Count);

        NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer)
            return;

        Debug.Log($"Client {clientId} disconnected.");

        StartCoroutine(UpdatePlayerCountAfterDisconnect());
    }

    private IEnumerator UpdatePlayerCountAfterDisconnect()
    {
        // Let NGO finish removing the disconnected client first.
        yield return null;

        int remainingPlayers =
            NetworkManager.Singleton.ConnectedClientsList.Count;

        Debug.Log($"Players remaining: {remainingPlayers}");

        GameManager.Instance.SetPlayerCount(remainingPlayers);
    }
}