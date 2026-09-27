using TMPro;
using UnityEngine;

public class ScoreboardRow : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text deathsText;
    [SerializeField] private TMP_Text roundsWonText;

    private PlayerStats _playerStats;

    public void Initialize(PlayerStats playerStats)
    {
        _playerStats = playerStats;

        Color rowColor = _playerStats.IsOwner ? Color.green : Color.white;

        playerNameText.color = rowColor;
        killsText.color = rowColor;
        deathsText.color = rowColor;
        roundsWonText.color = rowColor;

        // Set the current values immediately
        UpdatePlayerName("", _playerStats.PlayerName.Value);
        UpdateKills(0, _playerStats.Kills.Value);
        UpdateDeaths(0, _playerStats.Deaths.Value);
        UpdateRoundsWon(0, _playerStats.RoundsWon.Value);

        // Listen for future changes
        _playerStats.PlayerName.OnValueChanged += UpdatePlayerName;
        _playerStats.Kills.OnValueChanged += UpdateKills;
        _playerStats.Deaths.OnValueChanged += UpdateDeaths;
        _playerStats.RoundsWon.OnValueChanged += UpdateRoundsWon;
    }

    private void UpdatePlayerName(Unity.Collections.FixedString64Bytes oldValue,
        Unity.Collections.FixedString64Bytes newValue)
    {
        playerNameText.text = newValue.ToString();
    }

    private void UpdateKills(int oldValue, int newValue)
    {
        killsText.text = newValue.ToString();
    }

    private void UpdateDeaths(int oldValue, int newValue)
    {
        deathsText.text = newValue.ToString();
    }

    private void UpdateRoundsWon(int oldValue, int newValue)
    {
        roundsWonText.text = newValue.ToString();
    }

    private void OnDestroy()
    {
        if (_playerStats == null)
            return;

        _playerStats.PlayerName.OnValueChanged -= UpdatePlayerName;
        _playerStats.Kills.OnValueChanged -= UpdateKills;
        _playerStats.Deaths.OnValueChanged -= UpdateDeaths;
        _playerStats.RoundsWon.OnValueChanged -= UpdateRoundsWon;
    }
}