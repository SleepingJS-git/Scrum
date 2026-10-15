
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class NumPlayerReady : MonoBehaviour
{
    private TMP_Text readyText;

    private void Awake()
    {
        readyText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        int ready = GameManager.Instance.ReadyCountUI.Value;
        int total = GameManager.Instance.TotalCountUI.Value;

        string display = $"{ready}/{total} Players ready";

        if (GameManager.GamePhase != GamePhase.Building)
            display = "";
                

        if (readyText.text != display)
            readyText.text = display;
    }
}