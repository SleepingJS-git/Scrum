using UnityEngine;
using UnityEngine.SceneManagement;

public enum MenuScreen
{
    Main,
    HostLobby,
    JoinLobby,
    LobbyRoom
}

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject hostLobbyScreen;
    [SerializeField] private GameObject joinLobbyScreen;
    [SerializeField] private GameObject lobbyRoomScreen;

    public void ShowScreen(MenuScreen screen)
    {
        mainScreen.SetActive(screen == MenuScreen.Main);
        hostLobbyScreen.SetActive(screen == MenuScreen.HostLobby);
        joinLobbyScreen.SetActive(screen == MenuScreen.JoinLobby);
        lobbyRoomScreen.SetActive(screen == MenuScreen.LobbyRoom);
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

    //Basic test for changing scenes with start game button
    [SerializeField] string sceneName = "LevelGrayboxing";
    public void StartGame()
    {
        SceneManager.LoadScene(sceneName);
    }

    private void Start()
    {
        ShowScreen(MenuScreen.Main);
    }

}