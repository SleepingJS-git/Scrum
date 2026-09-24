using System;
using TMPro;
using UnityEngine;
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
    [SerializeField] private TMP_Text connectionErrorText;


    private void Start()
    {
        LobbyManager.Instance.LobbyChanged += RefreshPlayerList;
    }


    private void OnDestroy()
    {
        // Important since this object gets destroyed when leaving the menu
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.LobbyChanged -= RefreshPlayerList;
        }
    }


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


    private void RefreshPlayerList()
    {
        ISession session = LobbyManager.Instance.CurrentSession;

        // Reset player slots
        for (int i = 0; i < playerSlots.Length; i++)
        {
            playerSlots[i].text = "Waiting for player...";
            playerSlots[i].color = otherPlayerColor;
        }

        if (session == null)
            return;


        // Fill player slots
        for (int i = 0;
             i < session.Players.Count && i < playerSlots.Length;
             i++)
        {
            var player = session.Players[i];

            string playerName = player.GetPlayerName();

            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Player " + (i + 1);
            }

            playerSlots[i].text = playerName;

            if (player.Id == session.CurrentPlayer.Id)
            {
                playerSlots[i].color = localPlayerColor;
            }
            else
            {
                playerSlots[i].color = otherPlayerColor;
            }
        }


        // Lobby title
        if (session.Players.Count > 0)
        {
            lobbyHeader.text =
                session.Players[0].GetPlayerName() + "'s Lobby";
        }
    }
}