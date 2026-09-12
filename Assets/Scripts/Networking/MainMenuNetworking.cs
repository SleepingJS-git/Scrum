using TMPro;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

public class MainMenuNetworking : MonoBehaviour
{
    [SerializeField] private TMP_Text hostCodeText;
    [SerializeField] private TMP_InputField joinCodeInput;

    //the session that this player is either hosting or joined to
    private ISession currentSession;

    //initializes unity cloud services
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("networking online");
    }

    //creates a lobby w/ code upon clicking host lobby button
    public async void HostLobby()
    {
        var options = new SessionOptions
        {
            MaxPlayers = 4
        }.WithRelayNetwork();

        currentSession =
            await MultiplayerService.Instance.CreateSessionAsync(options);

        hostCodeText.text = currentSession.Code;

        Debug.Log($"created lobby: {currentSession.Code}");
    }

    //joins a lobby if code is correct
    public async void JoinLobby()
    {
        string code = joinCodeInput.text.Trim().ToUpper();

        currentSession =
            await MultiplayerService.Instance.JoinSessionByCodeAsync(code);

        Debug.Log("joined lobby");
    }
}