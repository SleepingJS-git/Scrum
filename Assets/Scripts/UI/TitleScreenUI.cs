using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] string sceneName = "LevelGrayboxing";
    public void StartGame()
    {
        SceneManager.LoadScene(sceneName);
    }
}