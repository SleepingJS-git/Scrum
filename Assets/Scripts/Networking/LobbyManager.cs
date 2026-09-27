using System;
using System.Threading.Tasks;
using UnityEngine;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

/// <summary>
/// Manages Unity Multiplayer Services sessions and gives their state to the rest of the game.
/// Persists between scenes as a Singleton
/// </summary>
public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    /// <summary>
    /// The session this local player is currently in
    /// </summary>
    public ISession CurrentSession { get; private set; }

    public bool IsInLobby => CurrentSession != null;

    // Events used by other scripts instead of subscribing directly to ISession
    public event Action LobbyChanged;
    public event Action<string> PlayerJoined;
    public event Action<string> PlayerLeft;
    public event Action PlayerPropertiesChanged;

    private Task initializationTask;

    private const string LobbyReadyKey = "LobbyReady";


    /// <summary>
    /// Returns whether this local player owns the current session
    /// </summary>
    public bool IsLocalPlayerHost
    {
        get
        {
            if (CurrentSession == null)
                return false;

            return CurrentSession.Host == CurrentSession.CurrentPlayer.Id;
        }
    }


    /// <summary>
    /// Returns true when every player in the session has finished entering the lobby
    /// </summary>
    public bool AreAllPlayersLobbyReady
    {
        get
        {
            if (CurrentSession == null || CurrentSession.Players.Count == 0)
                return false;

            foreach (var player in CurrentSession.Players)
            {
                if (!player.Properties.TryGetValue(
                        LobbyReadyKey,
                        out var readyProperty))
                {
                    return false;
                }

                if (readyProperty.Value != "true")
                {
                    return false;
                }
            }

            return true;
        }
    }


    /// <summary>
    /// Sets this local player's shared LobbyReady property
    /// </summary>
    public async Task SetLocalLobbyReadyAsync(bool ready)
    {
        if (CurrentSession == null)
            return;

        CurrentSession.CurrentPlayer.SetProperty(
            LobbyReadyKey,
            new PlayerProperty(
                value: ready ? "true" : "false",
                visibility: VisibilityPropertyOptions.Member
            )
        );

        await CurrentSession.SaveCurrentPlayerDataAsync();

        // Refresh our own UI immediately after saving.
        LobbyChanged?.Invoke();
    }


    /// <summary>
    /// Creates the persistent LobbyManager singleton and begins service initialization
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        initializationTask = InitializeServicesAsync();
    }


    /// <summary>
    /// Initializes Unity Services and signs this player in anonymously if needed
    /// </summary>
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


    /// <summary>
    /// Creates and returns a new four-player Relay session
    /// </summary>
    public async Task<ISession> HostLobbyAsync()
    {
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


    /// <summary>
    /// Joins and returns the session matching the supplied lobby code
    /// </summary>
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


    /// <summary>
    /// Leaves the current session and clears its local state and listeners
    /// </summary>
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


    /// <summary>
    /// Stores a new current session and registers its event listeners
    /// </summary>
    private void SetCurrentSession(ISession session)
    {
        if (CurrentSession != null)
        {
            UnregisterSessionEvents(CurrentSession);
        }

        CurrentSession = session;

        RegisterSessionEvents(CurrentSession);

        LobbyChanged?.Invoke();
    }


    /// <summary>
    /// Subscribes LobbyManager to changes from the supplied session
    /// </summary>
    private void RegisterSessionEvents(ISession session)
    {
        session.PlayerJoined += OnPlayerJoined;
        session.PlayerHasLeft += OnPlayerHasLeft;
        session.PlayerPropertiesChanged += OnPlayerPropertiesChanged;
    }


    /// <summary>
    /// Removes LobbyManager's event subscriptions from the supplied session
    /// </summary>
    private void UnregisterSessionEvents(ISession session)
    {
        session.PlayerJoined -= OnPlayerJoined;
        session.PlayerHasLeft -= OnPlayerHasLeft;
        session.PlayerPropertiesChanged -= OnPlayerPropertiesChanged;
    }


    /// <summary>
    /// Receives the player ID of a player who joined and forwards the change
    /// </summary>
    private void OnPlayerJoined(string playerId)
    {
        Debug.Log($"player joined: {playerId}");

        PlayerJoined?.Invoke(playerId);
        LobbyChanged?.Invoke();
    }


    /// <summary>
    /// Receives the player ID of a player who left and forwards the change
    /// </summary>
    private void OnPlayerHasLeft(string playerId)
    {
        Debug.Log($"player left: {playerId}");

        PlayerLeft?.Invoke(playerId);
        LobbyChanged?.Invoke();
    }


    /// <summary>
    /// Forwards changes to shared player properties such as LobbyReady
    /// </summary>
    private void OnPlayerPropertiesChanged()
    {
        PlayerPropertiesChanged?.Invoke();
        LobbyChanged?.Invoke();
    }


    /// <summary>
    /// Returns whether the supplied player ID currently has LobbyReady set to true
    /// </summary>
    public bool IsPlayerLobbyReady(string playerId)
    {
        if (CurrentSession == null)
            return false;

        foreach (var player in CurrentSession.Players)
        {
            if (player.Id != playerId)
                continue;

            if (!player.Properties.TryGetValue(
                    LobbyReadyKey,
                    out var readyProperty))
            {
                return false;
            }

            return readyProperty.Value == "true";
        }

        return false;
    }
}