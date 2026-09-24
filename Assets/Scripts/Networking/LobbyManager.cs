using System;
using System.Threading.Tasks;
using UnityEngine;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    // The session this player is currently in
    public ISession CurrentSession { get; private set; }

    public bool IsInLobby => CurrentSession != null;

    // Our own events that other scripts can listen to
    public event Action LobbyChanged;
    public event Action<string> PlayerJoined;
    public event Action<string> PlayerLeft;
    public event Action PlayerPropertiesChanged;

    private Task initializationTask;

    /// <summary>
    /// create our singleton for lobbymanager
    /// </summary>
    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep this object when changing scenes
        DontDestroyOnLoad(gameObject);

        initializationTask = InitializeServicesAsync();
    }


    private async Task InitializeServicesAsync()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        await AuthenticationService.Instance.GetPlayerNameAsync();

        Debug.Log("networking online");
    }


    public async Task<ISession> HostLobbyAsync()
    {
        // Make sure Unity Services finished initializing first
        await initializationTask;

        var options = new SessionOptions
        {
            MaxPlayers = 4
        }
        .WithRelayNetwork()
        .WithPlayerName(VisibilityPropertyOptions.Member);

        ISession session =
            await MultiplayerService.Instance.CreateSessionAsync(options);

        SetCurrentSession(session);

        Debug.Log($"created lobby: {session.Code}");

        return session;
    }


    public async Task<ISession> JoinLobbyAsync(string code)
    {
        await initializationTask;

        var joinOptions = new JoinSessionOptions()
            .WithPlayerName(VisibilityPropertyOptions.Member);

        ISession session =
            await MultiplayerService.Instance.JoinSessionByCodeAsync(
                code,
                joinOptions
            );

        SetCurrentSession(session);

        Debug.Log($"joined lobby: {session.Code}");

        return session;
    }


    public async Task LeaveLobbyAsync()
    {
        if (CurrentSession == null)
            return;

        ISession sessionLeaving = CurrentSession;

        await sessionLeaving.LeaveAsync();

        UnregisterSessionEvents(sessionLeaving);

        CurrentSession = null;

        LobbyChanged?.Invoke();

        Debug.Log("left lobby");
    }


    private void SetCurrentSession(ISession session)
    {
        // Just in case we're replacing an existing session
        if (CurrentSession != null)
        {
            UnregisterSessionEvents(CurrentSession);
        }

        CurrentSession = session;

        RegisterSessionEvents(CurrentSession);

        // Tell anything interested that lobby information now exists
        LobbyChanged?.Invoke();
    }


    private void RegisterSessionEvents(ISession session)
    {
        session.PlayerJoined += OnPlayerJoined;
        session.PlayerHasLeft += OnPlayerHasLeft;
        session.PlayerPropertiesChanged += OnPlayerPropertiesChanged;
    }


    private void UnregisterSessionEvents(ISession session)
    {
        session.PlayerJoined -= OnPlayerJoined;
        session.PlayerHasLeft -= OnPlayerHasLeft;
        session.PlayerPropertiesChanged -= OnPlayerPropertiesChanged;
    }


    private void OnPlayerJoined(string playerId)
    {
        Debug.Log($"player joined: {playerId}");

        PlayerJoined?.Invoke(playerId);
        LobbyChanged?.Invoke();
    }


    private void OnPlayerHasLeft(string playerId)
    {
        Debug.Log($"player left: {playerId}");

        PlayerLeft?.Invoke(playerId);
        LobbyChanged?.Invoke();
    }


    private void OnPlayerPropertiesChanged()
    {
        PlayerPropertiesChanged?.Invoke();
        LobbyChanged?.Invoke();
    }
}