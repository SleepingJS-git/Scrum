using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class Scoreboard : MonoBehaviour
{
    public static Scoreboard Instance { get; private set; }

    [SerializeField] private ScoreboardRow scoreboardRowPrefab;
    [SerializeField] private Transform rowsParent;

    private CanvasGroup _canvasGroup;

    private readonly Dictionary<ulong, ScoreboardRow> _playerRows = new();

    private void Awake()
    {
        Instance = this;

        _canvasGroup = GetComponent<CanvasGroup>();

        SetScoreboardVisible(false);
    }

    private void Start()
    {
        PlayerStats[] players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);

        foreach (PlayerStats playerStats in players)
        {
            RegisterPlayer(playerStats);
        }
    }

    private void Update()
    {
        bool tabHeld =
            Keyboard.current != null &&
            Keyboard.current.tabKey.isPressed;

        SetScoreboardVisible(tabHeld);
    }

    private void SetScoreboardVisible(bool visible)
    {
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible;
    }

    public void RegisterPlayer(PlayerStats playerStats)
    {
        ulong clientId = playerStats.OwnerClientId;

        // Don't create the same player's row twice
        if (_playerRows.ContainsKey(clientId))
            return;

        ScoreboardRow row = Instantiate(scoreboardRowPrefab, rowsParent);

        row.Initialize(playerStats);

        _playerRows.Add(clientId, row);
    }

    public void UnregisterPlayer(PlayerStats playerStats)
    {
        ulong clientId = playerStats.OwnerClientId;

        if (_playerRows.TryGetValue(clientId, out ScoreboardRow row))
        {
            Destroy(row.gameObject);
            _playerRows.Remove(clientId);
        }
    }
}