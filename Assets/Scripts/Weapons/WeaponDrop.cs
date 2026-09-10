using Unity.Netcode;
using UnityEngine;

public class WeaponDrop : NetworkBehaviour
{
    public Rigidbody rb;
    public float throwForce;
    public float upForce;
    private RuntimeWeapon _runtimeWeapon;
    [HideInInspector]
    public NetworkVariable<ulong> weaponId = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<int> currentBullets = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<int> reserveBullets = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<bool> isDropped = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public void Init(ulong id, bool dropped)
    {
        weaponId.Value = id;
        isDropped.Value = dropped;
    }
    public override void OnNetworkSpawn()
    {
        Instantiate(WeaponDatabase.GetWeapon(weaponId.Value).weaponModel, transform);

        if (!isDropped.Value) Destroy(rb);
    }
    public void CreateWeapon(WeaponData d)
    {
        currentBullets.Value = d.bulletCount;
        reserveBullets.Value = (d.magCount - 1) * d.bulletCount;
    }

    public void CreateWeapon(int c, int r, Quaternion rot)
    {
        currentBullets.Value = c;
        reserveBullets.Value = r;
        rb.linearVelocity = Vector3.zero;

        Vector3 finalForce = (rot * Vector3.forward * throwForce) + (rot * Vector3.up * upForce);
        rb.AddForce(finalForce, ForceMode.Impulse);

        Vector3 finalTorque = (Helper.RandomTorque(1f, 3f) * .25f) + (rot * Vector3.forward);
        rb.AddTorque(finalTorque, ForceMode.Impulse);
    }

    public void Equip(Player player)
    {
        Debug.Log("Equip was called. IsOwner?: " + IsOwner);
        EquipServerRpc(currentBullets.Value, reserveBullets.Value);
    }


    [Rpc(SendTo.Server)]
    private void EquipServerRpc(int currentBullets, int reserveBullets, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            EquipClientRpc(currentBullets, reserveBullets, clientId);

            NetworkObject.Despawn();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EquipClientRpc(int currentBullets, int reserveBullets, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId)
            return;

        Player player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();

        player.combat.EquipWeapon(
            new RuntimeWeapon(WeaponDatabase.GetWeapon(weaponId.Value),
            currentBullets,
            reserveBullets
        ));
    }
}
