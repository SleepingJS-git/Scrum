using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class JumpAudio : NetworkBehaviour
{
    [Header("Jump")]
    [SerializeField] private AudioSource jumpSource;
    [SerializeField] private AudioClip jumpClip;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float ownJumpVolume = 0.35f;
    [SerializeField, Range(0f, 1f)] private float otherJumpVolume = 0.8f;

    [Header("Pitch")]
    [SerializeField, Range(0.5f, 1.5f)] private float minPitch = 0.95f;
    [SerializeField, Range(0.5f, 1.5f)] private float maxPitch = 1.05f;

    public override void OnNetworkSpawn()
    {
        if (jumpSource == null) jumpSource = GetComponent<AudioSource>();

        // Quieter and 2D for yourself, spatialized for everyone else.
        jumpSource.spatialBlend = IsOwner ? 0f : 1f;
        jumpSource.volume = IsOwner ? ownJumpVolume : otherJumpVolume;
        jumpSource.playOnAwake = false;
        jumpSource.loop = false;
        jumpSource.dopplerLevel = 0f;
    }

    // Call once when the owning player's jump is actually performed.
    public void PlayJump()
    {
        if (!IsSpawned || !IsOwner || !IsClient || jumpClip == null) return;

        float pitch = Random.Range(Mathf.Min(minPitch, maxPitch), Mathf.Max(minPitch, maxPitch));

        PlayJumpSound(pitch);
        JumpServerRpc(pitch);
    }

    [ServerRpc]
    private void JumpServerRpc(float pitch)
    {
        JumpClientRpc(pitch);
    }

    [ClientRpc]
    private void JumpClientRpc(float pitch)
    {
        if (IsOwner) return;

        PlayJumpSound(pitch);
    }

    private void PlayJumpSound(float pitch)
    {
        if (jumpSource == null || jumpClip == null) return;

        jumpSource.pitch = pitch;
        jumpSource.PlayOneShot(jumpClip);
    }
}