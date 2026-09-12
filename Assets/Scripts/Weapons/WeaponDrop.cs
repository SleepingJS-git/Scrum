using Unity.Netcode;
using UnityEngine;

public class WeaponDrop : NetworkBehaviour
{
    public Rigidbody rb;
    public float throwForce;
    public float upForce;

    /*
        Network Variables - The server and clients share these variables
        These variables can be called on ServerRpcs [rpc(SendTo.Server)]
        Any changes made to these variables automatically get updated to
        all the other clients.
        If a non-NetworkVariable is changed on the server, only the server-side
        variable is changed and not the client's. 
    */
    [HideInInspector]
    public NetworkVariable<ulong> weaponID = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    [HideInInspector]
    public NetworkVariable<int> currentBullets = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    [HideInInspector]
    public NetworkVariable<int> reserveBullets = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private int _currentBullets;
    private int _reserveBullets;
    private bool _isDropped;
    private ulong _weaponID;
    /// <summary>
    /// [Called by Server]
    /// Give the WeaponDrop the WeaponID and whether if it was dropped by a player or created by a spawner
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dropped"></param>
    public void Init(ulong id, bool dropped)
    {
        _weaponID = id;
        _isDropped = dropped;
    }

    /// <summary>
    /// Once the object is spawned on the network, create the 3d model of the weapon
    /// Then update the NetworkVariables if the code is being ran by the Server
    /// </summary>
    public override void OnNetworkSpawn()
    {
        Instantiate(WeaponDatabase.GetWeapon(weaponID.Value).weaponModel, transform);

        if (!_isDropped) Destroy(rb);

        if (!IsServer) return;
            weaponID.Value = _weaponID;
            currentBullets.Value = _currentBullets;
            reserveBullets.Value = _reserveBullets;
    }
    /// <summary>
    /// [Called By Server] (WeaponSpawner)
    /// Create new bullet and reserve counts. This is a freshly spawned gun.
    /// </summary>
    /// <param name="d"></param>
    public void CreateWeapon(WeaponData d)
    {
        _currentBullets = d.bulletCount;
        _reserveBullets = (d.magCount - 1) * d.bulletCount;
    }

    /// <summary>
    /// [Called By ServerRpc] (DropServerRpc)
    /// Save the given bullet counts then add forces and torque because this was a thrown gun.
    /// </summary>
    /// <param name="d"></param>
    public void CreateWeapon(int c, int r, Quaternion rot)
    {
        _currentBullets = c;
        _reserveBullets = r;
        rb.linearVelocity = Vector3.zero;

        Vector3 finalForce = (rot * Vector3.forward * throwForce) + (rot * Vector3.up * upForce);
        rb.AddForce(finalForce, ForceMode.Impulse);

        Vector3 finalTorque = (Helper.RandomTorque(1f, 3f) * .25f) + (rot * Vector3.forward);
        rb.AddTorque(finalTorque, ForceMode.Impulse);
    }

    /// <summary>
    /// The player interacts with the WeaponDrop, request the server to equip it to the player.
    /// </summary>
    /// <param name="player"></param>
    public void Equip(Player player)
    {
        EquipServerRpc();
    }

    /// <summary>
    /// The server updates the sender client's weaponhandler with the stats of this drop
    /// </summary>
    /// <param name="rpcParams"></param>
    [Rpc(SendTo.Server)]
    private void EquipServerRpc(RpcParams rpcParams = default)
    {
        // Find the server's copy of the client who sent the request.
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            Weapon weapon = client.PlayerObject.GetComponent<Weapon>();

            // Update the player's weaponhandler with the networkvalues.
            weapon.Init(WeaponDatabase.GetWeapon(weaponID.Value), currentBullets.Value, reserveBullets.Value);
            
            // Send a client rpc with id of the client who sent the request.
            EquipClientRpc(clientId);

            // Despawn the drop
            NetworkObject.Despawn();
        }
    }

    /// <summary>
    /// Actually "Equip" the weapon for this player on all of the clients.
    /// If you couldn't tell, ClientRpcs call this method on all client's computers
    /// for their version of this player.
    /// </summary>
    /// <param name="clientId"></param>
    [Rpc(SendTo.ClientsAndHost)]
    private void EquipClientRpc(ulong clientId)
    {
        PlayerCombat combat;
        WeaponData data = WeaponDatabase.GetWeapon(weaponID.Value);
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            combat = client.PlayerObject.GetComponent<PlayerCombat>();

            // If this client is the one who requested the equip, 
            // then give their player object the fps weapon overlay
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                combat.EquipWeapon(data, currentBullets.Value, reserveBullets.Value);
            }

            // If this client is not the one who requested the equip, 
            // then give the player whose client requested the equip the 3d model body
            else
            {
                combat.EquipWeapon3D(data);
            }
        }
    }
}
