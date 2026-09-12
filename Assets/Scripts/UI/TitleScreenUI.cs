using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public enum MenuScreen
{
    Main,
    HostLobby,
    JoinLobby,
    LobbyRoom,
    LoadingLobby
}

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject hostLobbyScreen;
    [SerializeField] private GameObject joinLobbyScreen;
    [SerializeField] private GameObject lobbyRoomScreen;
    [SerializeField] private GameObject loadingLobbyScreen;

    public void ShowScreen(MenuScreen screen)
    {
        mainScreen.SetActive(screen == MenuScreen.Main);
        hostLobbyScreen.SetActive(screen == MenuScreen.HostLobby);
        joinLobbyScreen.SetActive(screen == MenuScreen.JoinLobby);
        lobbyRoomScreen.SetActive(screen == MenuScreen.LobbyRoom);
        loadingLobbyScreen.SetActive(screen == MenuScreen.LoadingLobby);
    }

    public void ShowMain()
    {
        ShowScreen(MenuScreen.Main);
    }

    public void ShowHostLobby()
    {
        ShowScreen(MenuScreen.HostLobby);
    }

    public void ShowJoinLobby()
    {
        ShowScreen(MenuScreen.JoinLobby);
    }

    public void ShowLobbyRoom()
    {
        ShowScreen(MenuScreen.LobbyRoom);
    }

    public void ShowLoadingLobby()
    {
        ShowScreen(MenuScreen.LoadingLobby);
    }

    //Start game from lobby
    [SerializeField] string sceneName = "SceneConnectionTest";
    public void StartGame()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("No NetworkManager found!");
            return;
        }

        // Only the server/host should initiate the scene change
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only the host can start the game.");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }

    private void Start()
    {
        ShowScreen(MenuScreen.Main);
    }

}