using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] private TitleScreen titleScreen;
    [SerializeField] private TMP_Dropdown levelDropdown;

    private bool? previousHostState;

    private void OnEnable()
    {
        previousHostState = null;
    }

    private void Update()
    {
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;

        if (previousHostState == isHost) return;

        previousHostState = isHost;
        levelDropdown.gameObject.SetActive(isHost);

        if (isHost)
            SelectLevel(levelDropdown.value);
    }

    public void SelectLevel(int index)
    {
        if (index < 0 || index >= levelDropdown.options.Count) return;

        titleScreen.SetSceneName(levelDropdown.options[index].text);
    }
}