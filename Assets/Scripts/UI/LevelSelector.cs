using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class LevelSelector : MonoBehaviour
{
    [System.Serializable]
    public class LevelOption
    {
        public Button button;
        public string sceneName;
    }

    [SerializeField] private TitleScreen titleScreen;
    [SerializeField] private LevelOption[] levels;

    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private void Start()
    {
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;

        gameObject.SetActive(isHost);

        if (!isHost) return;

        for (int i = 0; i < levels.Length; i++)
        {
            int index = i;
            levels[i].button.onClick.AddListener(() => SelectLevel(index));
        }

        SelectLevel(0);
    }

    private void SelectLevel(int index)
    {
        titleScreen.SetSceneName(levels[index].sceneName);

        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].button.image.color = i == index ? selectedColor : unselectedColor;
        }
    }
}