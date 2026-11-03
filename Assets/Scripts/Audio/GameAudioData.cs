using UnityEngine;

[CreateAssetMenu(fileName = "GameAudioData", menuName = "Audio/GameAudioData")]
public class GameAudioData : ScriptableObject
{
    public AudioClip hitmarkerSound;
    public AudioClip[] deathSounds;
    public AudioClip[] menuMusic;

    [Header("UI Sounds")]
    public AudioClip[] buttonHoverSounds;
    public AudioClip[] buttonClickSounds;

    [Header("Placement Sounds")]
    public AudioClip[] placementSounds;
    public AudioClip invalidPlacementSound;
}
