using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The main class that stores references to all the other player scripts and also controls them.
/// If you want to reference a child script, it must go through here.
/// </summary>
public class Player : Entity
{
    [HideInInspector] public PlayerInputHandler input;
    [HideInInspector] public PlayerCombat combat;
    [HideInInspector] public PlayerMovement move;
    [HideInInspector] public PlayerLook look;
    [HideInInspector] public PlayerInteraction interaction;

    // I know this is messy. This is a test. Eventually I want to have a singleton manager have a variable
    // so the player can reference it themselves.
    [SerializeField] private PlayerHud playerHudPrefab;
    [SerializeField] private PlayerHud _hud;
    public Transform PlayerCam => look.cam.transform;
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
        input = GetComponent<PlayerInputHandler>();
        combat = GetComponent<PlayerCombat>();
        move = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();
        interaction = GetComponent<PlayerInteraction>();

        if (IsOwner)
        {
            // For Debug rn, toggle first person immediately
            ToggleFirstPerson(true);

            _hud = Instantiate(playerHudPrefab);

            // Request the server to spawn player at specific spawn point
            SpawnServerRpc();
        }

        // Check if the computer running this script is the client.
        // If it is then IsOwner = true.
        input.Init(IsOwner);
        move.Init();
        look.Init(IsOwner);
        combat.Init(IsOwner, _hud);
        interaction.Init(look.cam.transform, _hud);
    }

    void Update()
    {
        // If not owner, then don't move something that isn't yours
        if (!IsOwner) return;
        
        move.Move(input.MoveInput);
        combat.PrimaryInput(input.PrimaryInput);
        interaction.Interaction();
    }

    void LateUpdate()
    {
        // If not owner, then don't move something that isn't yours
        if (!IsOwner) return;
        look.Look(input.LookInput());
    }

    /// <summary>
    /// Toggles the control of first person.
    /// True = the player gains control of FP Movement
    /// False = the player loses control of FP Movement
    /// Controls will be overwrited outside of this script.
    /// </summary>
    /// <param name="toFPS"></param>
    public void ToggleFirstPerson(bool toFPS)
    {
        Cursor.visible = !toFPS;
        Cursor.lockState = toFPS ? CursorLockMode.Locked: CursorLockMode.Confined;
    }


    /// <summary>
    /// Server uses the sender client's id to figure out which
    /// spawn point is to be used.
    /// </summary>
    /// <param name="rpcParams"></param>
    [Rpc(SendTo.Server)]
    public void SpawnServerRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Vector3 pos = PlayerSpawner.GetSpawnPoint(clientId).transform.position;
        pos.y+= 1f;     // up 1 unit so they dont clip through the floor
        
        // Send request back to client to make changes
        SpawnClientRpc(pos);
    }

    /// <summary>
    /// The client is told to move a position by the server.
    /// </summary>
    /// <param name="pos"></param>
    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnClientRpc(Vector3 pos)
    {
        if (!IsOwner)
            return;

        transform.position = pos;
    }
}
