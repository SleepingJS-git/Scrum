using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public ConnectUI connectUI;
    [SerializeField] private GamePhase gamePhase;
    public static GamePhase GamePhase => Instance.gamePhase;
    [SerializeField] private int numOfPlayers;
    [SerializeField] private TMP_Text roundWinnerText;
    [SerializeField] private CanvasGroup roundWinnerCanvasGroup;
    private int _loadedPlayers;
    private int _deadPlayers;
    private int _readyPlayers;
    private int _resetPlayers;
    public NetworkVariable<int> ReadyCountUI = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> TotalCountUI = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<FixedString512Bytes> debugInfo = new(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> Net_TotalTime = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private float maxBuildTime = 30f;
    [SerializeField] private float maxRoundTime = 90f;
    [SerializeField] private Image timerBar;
    [SerializeField] private TextMeshProUGUI timerText;
    private Coroutine timerRoutine;
    public bool SuddenDeath {get; private set;}
    public List<WeaponDrop> droppedWeapons = new List<WeaponDrop>();
    void Awake()
    {
        Instance = this;
    }

    void Update()
    {

    }

    public override void OnNetworkSpawn()
    {

        // Test, eventually there needs to be code to check if all
        // players spawned in.

        MusicManager.Instance?.StopMusic();

        Net_TotalTime.OnValueChanged += UpdateTimerUI;
        if (!IsServer) return;
        ChangeGamePhaseServerRpc(GamePhase.Loading);
    }  

    private void UpdateTimerUI(int oldVal, int newVal)
    {
        timerText.text = newVal.ToString();

        float perc = 0f;
        if (GamePhase == GamePhase.Building)
            perc = (float) newVal / maxBuildTime;
        else if (GamePhase == GamePhase.Combat)
            perc = (float) newVal / maxRoundTime;
        else if (newVal == 0)
            timerText.text = "";
        else
            timerText.text = "";

        timerBar.fillAmount = perc;
    }
    /// <summary>
    /// Set player count, used in NetworkSpawner after spawning players in
    /// </summary>
    /// <param name="count"></param>
    public void SetPlayerCount(int count)
    {
        numOfPlayers = count;
        TotalCountUI.Value = numOfPlayers;
    }

    /// <summary>
    /// Make sure we are not referencing an old game manager when we load into a game after exiting once
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Called on Server.
    /// Player is loaded
    /// </summary>
    /// <param name="clientId"></param>
    public void PlayerLoaded(ulong clientId)
    {
        if (!IsServer) return;
        _loadedPlayers++;

        Debug.Log($"Players Joined: {_loadedPlayers} / {numOfPlayers}");

        if (_loadedPlayers == numOfPlayers)
        {
            ChangeGamePhaseServerRpc(GamePhase.Building);
        }
    }

    /// <summary>
    /// Called on server.
    /// This player has died.
    /// </summary>
    /// <param name="clientID"></param>
    public void PlayerDeath(ulong clientID)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientID, out NetworkClient client))
        {
            Player player = client.PlayerObject.GetComponent<Player>();

            if (player.isAlive.Value) return;   // Player is not dead.
            PlayerStats stats = client.PlayerObject.GetComponent<PlayerStats>();
            stats.AddDeath();

            if (!SuddenDeath) return;

            _deadPlayers++;

            Debug.Log("Dead Players: " + _deadPlayers);
            
            if (_deadPlayers >= numOfPlayers - 1)
            {
                ChangeGamePhaseServerRpc(GamePhase.EndOfCombat);
            }

        }

    }

    [Rpc(SendTo.Server)]
    public void PlayerResetServerRpc()
    {
        _resetPlayers++;

        Debug.Log($"Players Reset: {_resetPlayers} / {numOfPlayers}");

        if (_resetPlayers == numOfPlayers)
        {
            if (gamePhase != GamePhase.Building)
                ChangeGamePhaseServerRpc(GamePhase.Building);
        }
    }

    [Rpc(SendTo.Server)]
    public void PlayerReadyServerRpc()
    {
        _readyPlayers++;
        ReadyCountUI.Value = _readyPlayers;

        Debug.Log($"Players Ready: {_loadedPlayers} / {numOfPlayers}");

        if (_readyPlayers == numOfPlayers)
            ChangeGamePhaseServerRpc(GamePhase.Combat);
    }
    /// <summary>
    /// Debug Button
    /// </summary>
    public void ChangeToCombat()
    {
        ChangeGamePhaseServerRpc(GamePhase.Combat);
    }

    /// <summary>
    /// Debug Button
    /// </summary>
    public void ChangeToBuilding()
    {
        ChangeGamePhaseServerRpc(GamePhase.Building);
    }
    [Rpc(SendTo.Server)]
    private void ChangeGamePhaseServerRpc(GamePhase gp)
    {
        gamePhase = gp;
        switch (gamePhase)
        {
            case GamePhase.Combat:
                _readyPlayers = 0;
                ReadyCountUI.Value = 0;
                _resetPlayers = 0;
                _deadPlayers = 0;
                PlayerControllerRpc((int) gamePhase);
                StartCombatTimer();
                break;

            case GamePhase.Building:
                _readyPlayers = 0;
                ReadyCountUI.Value = 0;
                _resetPlayers = 0;
                _deadPlayers = 0;
                PlayerControllerRpc((int) gamePhase);
                StartBuildTimer();

                foreach(WeaponDrop wpn in droppedWeapons)
                    wpn.NetworkObject.Despawn();
                break;

            case GamePhase.EndOfCombat:
                SuddenDeath = false;
                _readyPlayers = 0;
                ReadyCountUI.Value = 0;
                _resetPlayers = 0;
                _deadPlayers = 0;
                if (timerRoutine != null)
                {
                    StopCoroutine(timerRoutine);
                    timerRoutine = null;
                }

                PlayerControllerRpc((int)gamePhase);
                Net_TotalTime.Value = 0;
                ShowRoundWinner();
                Invoke(nameof(ResetPlayers), 5f);
                break;
        }
    }

    private void StartBuildTimer()
    {
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        Net_TotalTime.Value = (int) maxBuildTime;
        timerRoutine = StartCoroutine(TimerRoutine(maxBuildTime));
    }

    private void StartCombatTimer()
    {
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        Net_TotalTime.Value = (int) maxRoundTime;
        timerRoutine = StartCoroutine(TimerRoutine(maxRoundTime));
    }

    private IEnumerator TimerRoutine(float maxTime)
    {
        float timeLeft = maxTime;
        int oldInt = (int) maxTime;
        
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;

            if (oldInt != (int) timeLeft)
                Net_TotalTime.Value = (int) timeLeft;

            yield return null;
            oldInt = (int) timeLeft;
        }

        Net_TotalTime.Value = 0;

        if (GamePhase == GamePhase.Building)
            ChangeGamePhaseServerRpc(GamePhase.Combat);
        else if (GamePhase == GamePhase.Combat)
        {
            SuddenDeath = true;
        }
    }

    private void ResetPlayers()
    {
        PlayerManager.ResetEvent();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerControllerRpc(int gamePhaseInt)
    {
        gamePhase = (GamePhase) gamePhaseInt;

        if (gamePhase == GamePhase.Building)
        {
            PlayerManager.Instance.ChangePov(1);
            roundWinnerCanvasGroup.alpha = 0f;
            connectUI.ResetReadyButton();
        }
        else if (gamePhase == GamePhase.Combat)
        {
            connectUI.readyButton.gameObject.SetActive(false);
            PlayerManager.Instance.ChangePov(0);
        }
    }


    public FixedString512Bytes DebugInfo()
    {
        //DebugInfoServerRpc();
        return debugInfo.Value;
    }

    //[Rpc(SendTo.Server)]
    //private void DebugInfoServerRpc()
    //{
    //    debugInfo.Value = $@"

    //    (Game Info)
    //    Game Phase: {GamePhase}
    //    Loaded Players: {_loadedPlayers} / {numOfPlayers}
    //    Ready Players: {_readyPlayers} / {numOfPlayers}
    //    Dead Players: {_deadPlayers} / {numOfPlayers}
    //    Revived Players: {_resetPlayers} / {numOfPlayers}
    //    ";
    //}

    /// <summary>
    /// Method for showing text declaring who won the round
    /// </summary>
    private void ShowRoundWinner()
    {
        int mostPoints = -1;
        Player winner = null;

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Player player = client.PlayerObject.GetComponent<Player>();

            if (player != null)
            {
                PlayerStats stats = client.PlayerObject.GetComponent<PlayerStats>();

                if (mostPoints < player.Stats.Kills.Value)
                {
                    mostPoints = player.Stats.Kills.Value;
                    winner = player;
                }
                
            }
        }

        if (winner != null)
        {
            winner.Stats.AddRoundWin();
            ShowRoundWinnerRpc(winner.Stats.PlayerName.Value.ToString());
        }
        else
            Debug.LogWarning("Could not find a winning player.");
    }

    /// <summary>
    /// Update the round winner text on each player's screen
    /// </summary>
    /// <param name="winnerName"></param>
    [Rpc(SendTo.ClientsAndHost)]
    private void ShowRoundWinnerRpc(string winnerName)
    {
        roundWinnerText.text = $"{winnerName} won the round!";
        roundWinnerCanvasGroup.alpha = 1f;
    }

}



public enum GamePhase
{
    Loading,
    Combat,
    EndOfCombat,
    Building
}
