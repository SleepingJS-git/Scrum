using Unity.Netcode;
using UnityEngine;

public class SlideAudio : NetworkBehaviour
{
    [Header("Slide")]
    [SerializeField] private AudioSource slideSource;
    [SerializeField] private AudioClip slideClip;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float ownSlideVolume = 0.35f;
    [SerializeField, Range(0f, 1f)] private float otherSlideVolume = 0.8f;

    [Header("Pitch")]
    [SerializeField, Range(0.5f, 1.5f)] private float minPitch = 0.95f;
    [SerializeField, Range(0.5f, 1.5f)] private float maxPitch = 1.05f;

    public override void OnNetworkSpawn()
    {
        if (slideSource == null) return;

        slideSource.spatialBlend = IsOwner ? 0f : 1f;
        slideSource.volume = IsOwner ? ownSlideVolume : otherSlideVolume;
        slideSource.playOnAwake = false;
        slideSource.loop = false;
        slideSource.dopplerLevel = 0f;
    }

    public void PlaySlide()
    {
        if (!IsSpawned || !IsOwner || !IsClient || slideSource == null || slideClip == null) return;

        float pitch = Random.Range(Mathf.Min(minPitch, maxPitch), Mathf.Max(minPitch, maxPitch));

        PlaySlideSound(pitch);
        SlideServerRpc(pitch);
    }

    [ServerRpc]
    private void SlideServerRpc(float pitch)
    {
        SlideClientRpc(pitch);
    }

    [ClientRpc]
    private void SlideClientRpc(float pitch)
    {
        if (IsOwner) return;

        PlaySlideSound(pitch);
    }

    private void PlaySlideSound(float pitch)
    {
        if (slideSource == null || slideClip == null) return;

        slideSource.pitch = pitch;
        slideSource.PlayOneShot(slideClip);
    }
}