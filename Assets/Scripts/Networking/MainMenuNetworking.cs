using TMPro;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

public class MainMenuNetworking : MonoBehaviour
{
    [SerializeField] private TMP_Text hostCodeText;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text[] playerSlots;
    [SerializeField] private Color localPlayerColor = Color.green;
    [SerializeField] private Color otherPlayerColor = Color.white;
    [SerializeField] private TitleScreen titleScreenController;
    [SerializeField] private TMP_Text lobbyHeader;


    //the session that this player is either hosting or joined to
    private ISession currentSession;

    //initializes unity cloud services
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await AuthenticationService.Instance.GetPlayerNameAsync();

        Debug.Log("networking online");
    }


    //creates a lobby w/ code upon clicking host lobby button
    public async void HostLobby()
    {
        var options = new SessionOptions
        {
            MaxPlayers = 4
        }
        .WithRelayNetwork()
        .WithPlayerName(VisibilityPropertyOptions.Member);

        currentSession =
            await MultiplayerService.Instance.CreateSessionAsync(options);

        hostCodeText.text = currentSession.Code;

        Debug.Log($"created lobby: {currentSession.Code}");

        RegisterSessionEvents();
        RefreshPlayerList();

        titleScreenController.ShowLobbyRoom();
    }

    //joins a lobby if code is correct
    public async void JoinLobby()
    {
        string code = joinCodeInput.text.Trim().ToUpper();

        var joinOptions = new JoinSessionOptions()
            .WithPlayerName(VisibilityPropertyOptions.Member);

        currentSession =
            await MultiplayerService.Instance.JoinSessionByCodeAsync(code, joinOptions);

        Debug.Log("joined lobby");

        RegisterSessionEvents();
        RefreshPlayerList();

        titleScreenController.ShowLobbyRoom();
    }

    //Use this method to refresh lobby data for UI
    private void RefreshPlayerList()
    {
        //Resets player slots
        for (int i = 0; i < playerSlots.Length; i++)
        {
            playerSlots[i].text = "Waiting for player...";
            playerSlots[i].color = otherPlayerColor;
        }

        //add players to slots
        for (int i = 0; i < currentSession.Players.Count && i < playerSlots.Length; i++)
        {
            var player = currentSession.Players[i];

            string playerName = player.GetPlayerName();

            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Player " + (i + 1);
            }

            playerSlots[i].text = playerName;

            //color slots based on who's who
            if (player.Id == currentSession.CurrentPlayer.Id)
            {
                playerSlots[i].color = localPlayerColor;
            }
            else
            {
                playerSlots[i].color = otherPlayerColor;
            }
        }
        
        //Add the lobby title
        lobbyHeader.text = currentSession.Players[0].GetPlayerName() + "'s Lobby";
    }

    //Listeners for when lobby updates (player joins or leaves, or a player's name changed somehow), all listeners refresh the player list when one of these happens
    private void RegisterSessionEvents()
    {
        currentSession.PlayerJoined += OnPlayerJoined;
        currentSession.PlayerHasLeft += OnPlayerHasLeft;
        currentSession.PlayerPropertiesChanged += OnPlayerPropertiesChanged;
    }

    private void OnPlayerJoined(string playerId)
    {
        Debug.Log($"Player joined: {playerId}");
        RefreshPlayerList();
    }

    private void OnPlayerHasLeft(string playerId)
    {
        Debug.Log($"Player left: {playerId}");
        RefreshPlayerList();
    }

    private void OnPlayerPropertiesChanged()
    {
        RefreshPlayerList();
    }


}