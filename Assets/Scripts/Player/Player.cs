using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The main class that stores references to all the other player scripts and also controls them.
/// If you want to reference a child script, it must go through here.
/// </summary>
public class Player : NetworkBehaviour
{
    [HideInInspector] public PlayerInputHandler input;
    [HideInInspector] public PlayerMovement move;
    [HideInInspector] public PlayerLook look;

    NetworkVariable<int> HEALTH = new NetworkVariable<int>(
        100, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner
    );

    /// <summary>
    /// Initializes all the player scripts. Eventually, we'll have to move everything into networking's version of Start()
    /// </summary>
    public override void OnNetworkSpawn()
    {
        input = GetComponent<PlayerInputHandler>();
        move = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();

        // For Debug rn, toggle first person immediately
        ToggleFirstPerson(true);

        input.Init(IsOwner);
        move.Init();
        look.Init(IsOwner);
    }

    void Update()
    {
        if (!IsOwner) return;
        
        move.Move(input.MoveInput);
    }

    void LateUpdate()
    {
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
}
