using UnityEngine;
using System.Collections;


public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private GameAudioData audioData;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float fadeDuration = 5.0f;
    void Start()
    {

        audioSource.clip = audioData.menuMusic[0];
        if (audioSource != null)
            PlayMusic();

    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic()
    {
        audioSource.clip = audioData.menuMusic[0];
        FadeCouroutine(fadeDuration, true);
    }

    public void StopMusic()
    {
        FadeCouroutine(fadeDuration, false);
    }

    /// <summary>
    /// Couroutine for the fade, don't publicly call this method
    /// </summary>
    /// <param name="duration">Duration of the fade</param>
    /// <param name="fadeIn">Set bool to true for fade in, false for fade out </param>
    /// <returns></returns>
    private IEnumerator FadeCouroutine(float duration, bool fadeIn)
    {
        float startVolume = audioSource.volume;
        float currentTime = 0;

        if (fadeIn)
        {
            audioSource.Play();
        }

        while(currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, fadeIn ? 1.0f : 0.0f, currentTime / duration);
            yield return null;
        }

        if (!fadeIn)
        {
            audioSource.Stop();
        }
           
    }
}
