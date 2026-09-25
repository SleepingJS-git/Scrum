using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.UI;

/// <summary>
/// Represents the different menu panels controlled by TitleScreen
/// </summary>
public enum MenuScreen
{
    Main,
    HostLobby,
    JoinLobby,
    LobbyRoom,
    LoadingLobby,
    Tutorial
}

/// <summary>
/// Controls title-screen panels and the lobby Start Game button.
/// Uses LobbyManager state to determine whether this player is allowed to start the game
/// </summary>
public class TitleScreen : MonoBehaviour
{
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject hostLobbyScreen;
    [SerializeField] private GameObject joinLobbyScreen;
    [SerializeField] private GameObject lobbyRoomScreen;
    [SerializeField] private GameObject loadingLobbyScreen;
    [SerializeField] private GameObject tutorialScreen;

    [SerializeField] private Button startGameButton;
    [SerializeField] private CanvasGroup startGameCanvasGroup;

    [SerializeField] private string sceneName = "SceneConnectionTest";


    /// <summary>
    /// Shows the requested menu panel and hides the others
    /// </summary>
    public void ShowScreen(MenuScreen screen)
    {
        mainScreen.SetActive(screen == MenuScreen.Main);
        hostLobbyScreen.SetActive(screen == MenuScreen.HostLobby);
        joinLobbyScreen.SetActive(screen == MenuScreen.JoinLobby);
        lobbyRoomScreen.SetActive(screen == MenuScreen.LobbyRoom);
        loadingLobbyScreen.SetActive(screen == MenuScreen.LoadingLobby);
        tutorialScreen.SetActive(screen == MenuScreen.Tutorial);
    }


    // These methods are used by the main menu buttons to switch states with the OnClick() activator
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
        RefreshStartGameButton();
    }

    public void ShowLoadingLobby()
    {
        ShowScreen(MenuScreen.LoadingLobby);
    }

    public void ShowTutorial()
    {
        ShowScreen(MenuScreen.Tutorial);
    }


    /// <summary>
    /// Enables Start Game only for the host when every lobby player is ready
    /// </summary>
    private void RefreshStartGameButton()
    {
        if (LobbyManager.Instance == null ||
            LobbyManager.Instance.CurrentSession == null)
        {
            SetStartGameButtonEnabled(false);
            return;
        }

        bool canStart =
            LobbyManager.Instance.IsLocalPlayerHost &&
            LobbyManager.Instance.AreAllPlayersLobbyReady;

        SetStartGameButtonEnabled(canStart);
    }


    /// <summary>
    /// Changes the Start Game button's interactability and opacity
    /// </summary>
    private void SetStartGameButtonEnabled(bool enabled)
    {
        startGameButton.interactable = enabled;

        if (startGameCanvasGroup != null)
        {
            startGameCanvasGroup.alpha = enabled ? 1f : 0.5f;
        }
    }


    /// <summary>
    /// Starts the networked game if this player is the host and everyone is ready
    /// </summary>
    public void StartGame()
    {
        if (LobbyManager.Instance == null ||
            LobbyManager.Instance.CurrentSession == null)
        {
            Debug.LogWarning("Cannot start game without an active lobby.");
            return;
        }

        if (!LobbyManager.Instance.IsLocalPlayerHost)
        {
            Debug.LogWarning("Only the host can start the game.");
            return;
        }

        // Also check readiness here so the UI button is not the only protection
        if (!LobbyManager.Instance.AreAllPlayersLobbyReady)
        {
            Debug.LogWarning(
                "Cannot start game while a player is still connecting."
            );

            RefreshStartGameButton();
            return;
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("No NetworkManager found!");
            return;
        }

        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning(
                "Only the network server can initiate the scene change."
            );
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }


    /// <summary>
    /// Shows the initial menu and begins listening for lobby changes
    /// </summary>
    private void Start()
    {
        ShowScreen(MenuScreen.Main);

        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.LobbyChanged += RefreshStartGameButton;
        }

        RefreshStartGameButton();
    }


    /// <summary>
    /// Removes the lobby event listener when this UI is destroyed
    /// </summary>
    private void OnDestroy()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.LobbyChanged -= RefreshStartGameButton;
        }
    }
}