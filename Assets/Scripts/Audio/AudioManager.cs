using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private GameAudioData audioData;
    [SerializeField] private AudioSource uiAudioSource;

    [Header("Button Audio")]
    [SerializeField] private AudioSource buttonAudioSource;
    [SerializeField] private Vector2 buttonPitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField, Range(0f, 1f)] private float hoverVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float clickVolume = 0.7f;

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

    public void PlayHitmarker()
    {
        uiAudioSource.PlayOneShot(audioData.hitmarkerSound);
    }

    public int GetRandomDeathSoundIndex()
    {
        return Random.Range(0, audioData.deathSounds.Length);
    }

    public AudioClip GetDeathSound(int index)
    {
        return audioData.deathSounds[index];
    }

    public void PlayButtonHover()
    {
        PlayButtonSound(audioData.buttonHoverSounds, hoverVolume);
    }

    public void PlayButtonClick()
    {
        PlayButtonSound(audioData.buttonClickSounds, clickVolume);
    }

    private void PlayButtonSound(AudioClip[] clips, float volume)
    {
        if (buttonAudioSource == null || clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        if (clip == null)
            return;

        buttonAudioSource.Stop();
        buttonAudioSource.clip = clip;
        buttonAudioSource.pitch = Random.Range(buttonPitchRange.x, buttonPitchRange.y);
        buttonAudioSource.volume = volume;
        buttonAudioSource.Play();
    }
}