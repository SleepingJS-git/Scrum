using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class NetworkDuplicateGuard : MonoBehaviour
{
    private void Awake()
    {
        NetworkManager[] managers =
            FindObjectsByType<NetworkManager>(FindObjectsSortMode.None);

        foreach (NetworkManager manager in managers)
        {
            if (manager.gameObject == gameObject)
                continue;

            Debug.Log(
                $"NetworkManager already exists on {manager.gameObject.name}. " +
                $"Destroying duplicate {gameObject.name}."
            );

            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}