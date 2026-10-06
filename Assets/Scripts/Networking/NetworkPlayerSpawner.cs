using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkPlayerSpawner : NetworkBehaviour
{
    // Make sure the host is the only one giving player spawns
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;

        Debug.Log("NetworkSpawner is Awakened");
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
}