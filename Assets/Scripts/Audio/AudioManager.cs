using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private GameAudioData audioData;
    [SerializeField] private AudioSource uiAudioSource;

    private void Awake()
    {
        // If another AudioManager already exists, destroy this duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayHitmarker()
    {
        uiAudioSource.PlayOneShot(audioData.hitmarkerSound);
    }
}