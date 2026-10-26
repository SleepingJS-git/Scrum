using Unity.Netcode;
using UnityEngine;

public class FootstepAudio : NetworkBehaviour
{
    [Header("Footsteps")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField, Min(0.1f)] private float distancePerStep = 1.8f;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float ownFootstepVolume = 0.35f;
    [SerializeField, Range(0f, 1f)] private float otherFootstepVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float crouchingVolumeMultiplier = 0.4f;

    private Player main;
    private CharacterController controller;
    private PlayerInputHandler input;
    private Vector3 previousPosition;
    private float distanceSinceStep;
    private int lastClipIndex = -1;

    public override void OnNetworkSpawn()
    {
        main = GetComponentInParent<Player>();
        controller = GetComponentInParent<CharacterController>();
        input = GetComponentInParent<PlayerInputHandler>();
        previousPosition = main.transform.position;
        distanceSinceStep = 0f;

        //play footsteps quieter and 2d for yourself
        footstepSource.spatialBlend = IsOwner ? 0f : 1f;
        footstepSource.volume = IsOwner ? ownFootstepVolume : otherFootstepVolume;
        footstepSource.playOnAwake = false;
        footstepSource.loop = false;
        footstepSource.dopplerLevel = 0f;
    }

    private void LateUpdate()
    {
        if (!IsSpawned || !IsOwner || !IsClient) return;

        // Measure actual horizontal movement after the player has moved
        Vector3 displacement = main.transform.position - previousPosition;
        previousPosition = main.transform.position;
        displacement.y = 0f;

        float distance = displacement.magnitude;
        MovementState state = (MovementState)main.MoveState.Value;

        // Don't make ordinary footsteps while airborne, sliding, idle, or teleporting
        if (!main.CanMove || !controller.enabled || !controller.isGrounded || state == MovementState.Sliding ||
            input.MoveInput.sqrMagnitude < 0.0001f || distance <= 0.15f * Time.deltaTime || distance > 3f)
        {
            distanceSinceStep = 0f;
            return;
        }

        distanceSinceStep += distance;

        if (distanceSinceStep < distancePerStep) return;
        distanceSinceStep %= distancePerStep;

        if (footstepClips == null || footstepClips.Length == 0) return;

        // Choose a random clip without immediately repeating the previous one
        int clipIndex = Random.Range(0, footstepClips.Length);

        if (footstepClips.Length > 1 && clipIndex == lastClipIndex)
            clipIndex = (clipIndex + Random.Range(1, footstepClips.Length)) % footstepClips.Length;

        lastClipIndex = clipIndex;
        bool crouching = state == MovementState.Crouching;

        // Hear my own step immediately, then tell the other players
        PlayFootstep(clipIndex, crouching);
        FootstepServerRpc(clipIndex, crouching);
    }

    [ServerRpc]
    private void FootstepServerRpc(int clipIndex, bool crouching)
    {
        FootstepClientRpc(clipIndex, crouching);
    }

    [ClientRpc]
    private void FootstepClientRpc(int clipIndex, bool crouching)
    {
        if (IsOwner) return;

        PlayFootstep(clipIndex, crouching);
    }

    private void PlayFootstep(int clipIndex, bool crouching)
    {
        if (footstepClips == null || clipIndex < 0 || clipIndex >= footstepClips.Length || footstepClips[clipIndex] == null) return;

        float volume = crouching ? crouchingVolumeMultiplier : 1f;
        footstepSource.PlayOneShot(footstepClips[clipIndex], volume);
    }
}