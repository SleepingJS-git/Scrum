
using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelector : MonoBehaviour
{

    [SerializeField] private TitleScreen titleScreen;
    private string[] levels;
    [SerializeField] private TMP_Dropdown levelDropdown;

    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private bool? previousHostState;

    private void Start()
    {
        for (int i = 0; i < levelDropdown.options.Count; i++)
        {
            levels[i] = levelDropdown.options[i].text;
        }
    }

    private void OnEnable()
    {
        previousHostState = null;
    }

    //Fixed the stuff with buttons not appearing sometimes because the buttons would completely disable themselves based on the host async timings
    private void Update()
    {
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;

        if (previousHostState == isHost) return;

        bool wasHost = previousHostState == true;
        previousHostState = isHost;

        levelDropdown.gameObject.SetActive(isHost);

        SelectLevel(levelDropdown.value);
    }

    private void SelectLevel(int index)
    {
        titleScreen.SetSceneName(levels[index]);
    }
}