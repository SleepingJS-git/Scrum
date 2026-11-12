using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private GameAudioData audioData;
    [SerializeField] private AudioSource uiAudioSource;

    [Header("Button Audio")]
    [SerializeField] private AudioSource buttonHoverSource;
    [SerializeField] private AudioSource buttonClickSource;
    [SerializeField] private Vector2 buttonPitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField, Range(0f, 1f)] private float hoverVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float clickVolume = 0.7f;
    [Header("Placement Audio")]
    [SerializeField, Range(0f, 1f)] private float placementVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float invalidPlacementVolume = 1f;
    [SerializeField] private AudioSource placementAudioSource;
    [SerializeField] private AudioSource buildableBreakSourcePrefab;

    public void PlayBuildableBreakSound(Vector3 position)
    {
        if (buildableBreakSourcePrefab == null || buildableBreakSourcePrefab.clip == null)
            return;

        AudioSource source = Instantiate(buildableBreakSourcePrefab, position, Quaternion.identity);
        source.Play();
        Destroy(source.gameObject, source.clip.length);
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
        PlayButtonSound(buttonHoverSource, audioData.buttonHoverSounds, hoverVolume);
    }

    public void PlayButtonClick()
    {
        PlayButtonSound(buttonClickSource, audioData.buttonClickSounds, clickVolume);
    }

    private void PlayButtonSound(AudioSource source, AudioClip[] clips, float volume)
    {
        if (source == null || clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        if (clip == null)
            return;

        source.Stop();
        source.clip = clip;
        source.pitch = Random.Range(buttonPitchRange.x, buttonPitchRange.y);
        source.volume = volume;
        source.Play();
    }

    public void PlayPlacement()
    {
        AudioClip[] clips = audioData.placementSounds;

        if (placementAudioSource == null || clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        if (clip != null)
            placementAudioSource.PlayOneShot(clip, placementVolume);
    }

    public void PlayInvalidPlacement()
    {
        if (placementAudioSource != null && audioData.invalidPlacementSound != null)
            placementAudioSource.PlayOneShot(audioData.invalidPlacementSound, invalidPlacementVolume);
    }
}