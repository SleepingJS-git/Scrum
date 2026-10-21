using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The main class that stores references to all the other player scripts and also controls them.
/// If you want to reference a child script, it must go through here.
/// </summary>
public class Player : Entity
{
    // Components
    public PlayerInputHandler Input { get; private set; }
    public PlayerCombat Combat { get; private set; }
    public PlayerMovement Move { get; private set; }
    public PlayerLook Look { get; private set; }
    public PlayerInteraction Interaction { get; private set; }
    public PlayerBody Body { get; private set; }
    public PlayerStats Stats { get; private set; }
    [SerializeField] private AudioSource playerAudioSource;
    public DeathCamera deathCamera;

    // Player Hud - Gets set by PlayerManager
    private PlayerHud hud;
    public void SetHud(PlayerHud h) => this.hud = h; 

    // Random garbage
    public Transform PlayerCam => Look.cam.transform;
    public Vector2 LookInput {get; private set; }
    public bool CanMove { get; private set; }
    [SerializeField] private bool _initialized = false;

    public NetworkVariable<int> MoveState = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    /// <summary>
    /// When the object is spawned on the network, intialize these scripts.
    /// 
    /// Each connected player is a client that runs this script on their computer. So if there were 4 players, 
    /// then there would be 16 instances of this script running. If it was 2, then there would be 4.
    /// 
    /// For each player, it checks if this player script (out of all the others) is the one they are controlling.
    /// If this script is the one that is controlling the player's then IsOwner = true!
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // Value for health is set
        base.OnNetworkSpawn();
        Input = GetComponent<PlayerInputHandler>();
        Combat = GetComponent<PlayerCombat>();
        Move = GetComponent<PlayerMovement>();
        Look = GetComponent<PlayerLook>();
        Interaction = GetComponent<PlayerInteraction>();
        Body = GetComponent<PlayerBody>();
        Stats = GetComponent<PlayerStats>();

        // Check if the computer running this script is the client.
        // If it is then IsOwner = true.
        Input.Init(IsOwner);
        Body.Init(IsOwner);
        Move.Init(IsOwner);
        Look.Init(IsOwner);
        if (IsOwner)
        {
            PlayerManager.Instance.SetLocalPlayer(this);
            deathCamera.Init();

            Interaction.Init(Look.cam.transform, hud);
        }

        Combat.Init(IsOwner, hud);

        
        deathCamera.Toggle(false);
        _initialized = true;
    }

    void Update()
    {
        // If not owner, then don't move something that isn't yours
        if (!IsOwner) return;

        // No controls unless all values are true
        if (!_initialized || !isAlive.Value || !CanMove) return;

        Move.Move(Input.MoveInput);
        Combat.PrimaryInput(Input.PrimaryInput);
        Interaction.Interaction();
    }

    void LateUpdate()
    {
        // If not owner, then don't move something that isn't yours
        if (!IsOwner) return;

        // No controls unless all values are true
        if (!_initialized || !isAlive.Value || !CanMove) return;

        LookInput = Input.LookInput();
        Look.Look(LookInput);
        RotateRigAimRpc(Look.XRot);
    }

    /// <summary>
    /// Play death sound
    /// </summary>
    /// <param name="soundIndex"></param>
    public void PlayDeathSound()
    {
        // Instead of an Everyone Rpc, just add this method to OnDeathEffects in Inspector.
        int soundIndex = AudioManager.Instance.GetRandomDeathSoundIndex();
        AudioClip clip = AudioManager.Instance.GetDeathSound(soundIndex);
        playerAudioSource.spatialBlend = IsOwner ? 0f : 1f;
        playerAudioSource.PlayOneShot(clip);
    }

    public void ToggleDeathHud(bool isDead)
    {
        if (!IsOwner) return;
        hud.youAreDeadObj.SetActive(isDead);
    }

    /// <summary>
    /// [Called by Server] (This method is still being ran on the server)
    /// When the entity is hit, subtract health, then check if it is dead.
    /// </summary>
    /// <param name="damage"></param>
    public override void OnHit(OnHitData onHitData)
    {
        OnHitClientRpc(onHitData.damage, onHitData.sourceHit, onHitData.explosionForce, onHitData.upwardsModifier);
        
        
        base.OnHit(onHitData);
        UpdateHealthClientRpc(OwnerClientId);
    }

    /// <summary>
    /// Temporary rpc that makes it so ragdolls get pushed when dying for all clients.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="sourceHit"></param>
    /// <param name="upwardsModifier"></param>
    [Rpc(SendTo.Everyone)]
    private void OnHitClientRpc(int damage, Vector3 sourceHit, float explosionForce, float upwardsModifier)
    {
        Body.onHitData.explosionForce = explosionForce;
        Body.onHitData.sourceHit = sourceHit;
        Body.onHitData.upwardsModifier = upwardsModifier;

    }

    /// <summary>
    /// [Called by Server]
    /// Updates the client who got damaged.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateHealthClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            hud.health.SetText(health.Value.ToString());
            
            if (!isAlive.Value) hud.health.SetText("0");
        }
    }

    public void ToggleMove(bool canMove)
    {
        CanMove = canMove;
        Input.ToggleInput(canMove);
        Body.Play("IsMoving", false);
    }

    public void Revive()
    {
        deathCamera.Toggle(false);
        hud.health.text = health.Value.ToString();
        Combat.EmptyWeapon();
        ToggleDeathHud(false);
        Body.UnRagdoll();
        Body.Play("IsMoving", false);
        Move.OnDeathCollider(false);
        Move.StopVelocity();
    }

    [Rpc(SendTo.Server)]
    public void ChangeMoveStateServerRpc(int newMoveState)
    {
        if (MoveState.Value == newMoveState)
            return;

        int lastMoveState = MoveState.Value;
        MoveState.Value = newMoveState;
        ChangeMoveStateClientRpc(newMoveState, lastMoveState);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ChangeMoveStateClientRpc(int newMoveState, int lastMoveState)
    {
        switch((MovementState) newMoveState)
        {
            case MovementState.Standing:
                if (lastMoveState == (int) MovementState.Crouching || 
                    lastMoveState == (int)MovementState.Sliding)
                    Move.UnCrouch();
                
                Look.ResetFOV();
            break;
            case MovementState.Crouching:
                Move.Crouch();
                Look.ResetFOV();
            break;
            case MovementState.Sprinting:
                Look.SprintFOV();
            break;
            case MovementState.Sliding:
                Move.Slide();
            break;
            
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void RotateRigAimRpc(float xRotation)
    {
        Look.RotateRigAim(xRotation);
    }

    public void StartDeathCam()
    {
        
    }

    
    /// <summary>
    /// [Called by Server]
    /// Simulate an applied force to the player.
    /// CharacterController does not have AddForce(), and movement is determined by CharacterController.Move().
    /// </summary>
    /// <param name="dir">Direction of the force</param>
    /// <param name="force">Amount of Force</param>
    /// <param name="forceMode">The Force Mode -> See note at the bottom of script</param>
    /// <param name="rpcParams">This should be sent only to the client that is affected by the force</param>
    [Rpc(SendTo.SpecifiedInParams)]
    public void AddForceRpc(Vector3 dir, float force, int forceMode, RpcParams rpcParams = default)
    {
        Debug.Log("AddForceRPC IsOwner?: " + IsOwner);
        Debug.Log("OwnerID: " + OwnerClientId);
        if (!IsOwner) return; // just in case
        Move.AddForce(dir, force, (ForceMode) forceMode);
    }
}

/*
    For some reason the enums are like this. So when calling AddForceRPC, you must send in the integer
    Force = 0,
    Acceleration = 5,
    Impulse = 1,
    VelocityChange = 2

    or alternatively you can just do (int) ForceMode.Acceleration for example.
*/
