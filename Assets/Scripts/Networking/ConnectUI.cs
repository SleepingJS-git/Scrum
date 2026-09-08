using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    void Start()
    {
        hostButton.onClick.AddListener(HostButtonOnClick);
        clientButton.onClick.AddListener(ClientButtonOnClick);
    }

    /// <summary>
    /// The client that clicks this button joins as the host client
    /// </summary>
    private void HostButtonOnClick()
    {
        NetworkManager.Singleton.StartHost();
    }

    /// <summary>
    /// The client that clicks this button joins as a client
    /// </summary>
    private void ClientButtonOnClick()
    {
        NetworkManager.Singleton.StartClient();
    }
}
