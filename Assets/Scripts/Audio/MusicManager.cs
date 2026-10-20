using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{

    [SerializeField] private GameAudioData audioData;
    [SerializeField] private AudioSource audioSource;
    void Start()
    {

        audioSource.clip = audioData.menuMusic[0];
        if (audioSource != null)
            PlayMusic();

    }

    public void PlayMusic()
    {
        audioSource.clip = audioData.menuMusic[0];
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}
