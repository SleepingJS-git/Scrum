using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkPlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
    }

    private void OnLoadEventCompleted(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (sceneName != gameObject.scene.name)
            return;

        int spawnIndex = 0;

        foreach (ulong clientId in clientsCompleted)
        {
            // Prevent accidentally giving somebody two PlayerObjects
            if (NetworkManager.ConnectedClients[clientId].PlayerObject != null)
                continue;

            Transform spawnPoint = spawnPoints[
                spawnIndex % spawnPoints.Length
            ];

            GameObject player = Instantiate(
                playerPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            player.GetComponent<NetworkObject>()
                .SpawnAsPlayerObject(clientId, true);

            spawnIndex++;
        }

        NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
    }
}