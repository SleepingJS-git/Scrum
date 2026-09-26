using System;
using TMPro;
using UnityEngine;
using Unity.Services.Multiplayer;

/// <summary>
/// Handles the main menu's networking UI
/// Uses LobbyManager for session operations, then displays the resulting lobby data
/// </summary>
public class MainMenuNetworking : MonoBehaviour
{
    [SerializeField] private TMP_Text hostCodeText;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text[] playerSlots;
    [SerializeField] private TitleScreen titleScreenController;

    [SerializeField] private TMP_Text lobbyHeader;
    [SerializeField] private TMP_Text connectionErrorText;


    /// <summary>
    /// Refreshes the player list whenever LobbyManager reports a lobby change
    /// </summary>
    private void Start()
    {
        LobbyManager.Instance.LobbyChanged += RefreshPlayerList;
    }


    /// <summary>
    /// Removes lobby event listeners when this menu object is destroyed
    /// </summary>
    private void OnDestroy()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.LobbyChanged -= RefreshPlayerList;
        }
    }


    /// <summary>
    /// Creates a lobby, displays its information, and marks the host as ready
    /// </summary>
    public async void HostLobby()
    {
        connectionErrorText.text = "";

        try
        {
            ISession session =
                await LobbyManager.Instance.HostLobbyAsync();

            hostCodeText.text = session.Code;

            RefreshPlayerList();

            titleScreenController.ShowLobbyRoom();

            //ready means this client has completely reached the lobby screen
            await LobbyManager.Instance.SetLocalLobbyReadyAsync(true);
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to create lobby: {e.Error}");

            connectionErrorText.text =
                "Failed to create lobby. Please try again.";

            titleScreenController.ShowHostLobby();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create lobby: {e}");

            connectionErrorText.text =
                "Something went wrong. Please try again.";

            titleScreenController.ShowHostLobby();
        }
    }


    /// <summary>
    /// Joins the entered lobby code, displays the lobby, and marks this player as ready
    /// </summary>
    public async void JoinLobby()
    {
        connectionErrorText.text = "";

        string code = joinCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(code))
        {
            connectionErrorText.text =
                "Please enter a lobby code.";

            titleScreenController.ShowJoinLobby();

            return;
        }

        try
        {
            ISession session =
                await LobbyManager.Instance.JoinLobbyAsync(code);

            hostCodeText.text = session.Code;

            RefreshPlayerList();

            titleScreenController.ShowLobbyRoom();

            // The host may already see this player before this finishes
            await LobbyManager.Instance.SetLocalLobbyReadyAsync(true);
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to join lobby: {e.Error}");

            connectionErrorText.text =
                "Couldn't join lobby. Check the code and try again.";

            titleScreenController.ShowJoinLobby();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join lobby: {e}");

            connectionErrorText.text =
                "Something went wrong. Please try again.";

            titleScreenController.ShowJoinLobby();
        }
    }


    /// <summary>
    /// Removes this player from the current lobby
    /// </summary>
    public async void LeaveLobby()
    {
        connectionErrorText.text = "";

        try
        {
            await LobbyManager.Instance.LeaveLobbyAsync();
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to leave lobby: {e}");

            connectionErrorText.text =
                "Error leaving lobby. Please try again.";
        }
    }


    /// <summary>
    /// Updates the lobby player slots using the latest CurrentSession data
    /// </summary>
    private void RefreshPlayerList()
    {
        ISession session = LobbyManager.Instance.CurrentSession;

        // Reset every slot before filling occupied ones
        for (int i = 0; i < playerSlots.Length; i++)
        {
            playerSlots[i].text = "Waiting for player...";
            playerSlots[i].color = Color.white;
        }

        if (session == null)
            return;


        for (int i = 0;
             i < session.Players.Count && i < playerSlots.Length;
             i++)
        {
            var player = session.Players[i];

            bool isLocalPlayer =
                player.Id == session.CurrentPlayer.Id;

            bool isReady =
                LobbyManager.Instance.IsPlayerLobbyReady(player.Id);

            // Remote players appear in the session before their lobby screen has fully loaded
            if (!isLocalPlayer && !isReady)
            {
                playerSlots[i].text = "Player connecting...";
                playerSlots[i].color = Color.cyan;
                continue;
            }

            string playerName = player.GetPlayerName();

            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Player " + (i + 1);
            }

            playerSlots[i].text = playerName;

            if (isLocalPlayer)
            {
                playerSlots[i].color = Color.green;
            }
            else
            {
                playerSlots[i].color = Color.white;
            }
        }


        if (session.Players.Count > 0)
        {
            lobbyHeader.text =
                session.Players[0].GetPlayerName() + "'s Lobby";
        }
    }
}