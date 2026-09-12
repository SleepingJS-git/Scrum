using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// A component attached to the PlayerObject and is used to handle the weapon behaviors
/// </summary>
public class Weapon : NetworkBehaviour
{
    /*
        Network Variables - The server and clients share these variables
        These variables can be called on ServerRpcs [rpc(SendTo.Server)]
        Any changes made to these variables automatically get updated to
        all the other clients.
        If a non-NetworkVariable is changed on the server, only the server-side
        variable is changed and not the client's. 
    */
    public NetworkVariable<int> currentBullets = new(
        0,
        NetworkVariableReadPermission.Everyone,         // Every client can read these variables
        NetworkVariableWritePermission.Server           // Only the server can make changes
    );

    public NetworkVariable<int> reserveBullets = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> fireRate = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<ulong> weaponID = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [HideInInspector] public Transform firingPoint;     // This is given when the weapon is equipped
    // It checks current time is greater than the cached time plus the firerate
    /*
        When Caching (Is Not Ready):
        Time.Time = 20f, lastShootTime = 20f, fireRate.Value = 0.075f
        20f >= 20.075f -> False

        After Waiting (Is Ready):
        Time.Time = 30f, lastShootTime = 20f, fireRate.Value = 0.075f
        30f >= 20.075f -> True
    */
    public bool IsReady { get { return Time.time >= lastShootTime + fireRate.Value; } }
    public bool NeedsReload { get { return currentBullets.Value <= 0; } }
    public bool HasNoAmmo { get { return currentBullets.Value == 0 && reserveBullets.Value == 0; } }
    private float lastShootTime;
    public override void OnNetworkSpawn()
    {

    }

    /// <summary>
    /// [Called by ServerRpc] (DropWeapon.Equip)
    /// This makes it so that the server copy of this player equipped the weapon stats.
    /// This is so the server can perform the calculations for shooting.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="c"></param>
    /// <param name="r"></param>
    public void Init(WeaponData data, int c, int r)
    {
        fireRate.Value = data.fireRate;
        weaponID.Value = data.weaponID;
        currentBullets.Value = c;
        reserveBullets.Value = r;
    }

    /// <summary>
    /// [Called by Client] (PlayerCombat.FireWeapon)
    /// Sends a server request for the weapon type passed in.
    /// </summary>
    /// <param name="weaponType"></param>
    /// <param name="headPos"></param>
    /// <param name="aimDir"></param>
    public void Shoot(WeaponType weaponType, Vector3 headPos, Vector3 aimDir)
    {
        if (weaponType == WeaponType.Hitscan)
        {
            HitscanFireServerRpc(headPos, aimDir);
        }
    }

    /// <summary>
    /// Get the player object from the sender client ID
    /// </summary>
    /// <param name="clientID"></param>
    /// <returns>The server's copy of the player</returns>
    public Player GetPlayer(ulong clientID)
    {
        Player player = null;
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientID, out NetworkClient client))
        {
            player = client.PlayerObject.GetComponent<Player>();
        }
        return player;
    }

    /// <summary>
    /// Have the server process the hitscan bullet
    /// A Hitscan bullet starts from the head and lands at whatever they were aiming at.
    /// If it was an entity, then call OnHit.
    /// If it hit something, then call BulletTrail to visualize the shot.
    /// </summary>
    /// <param name="rpcParams"></param>
    [Rpc(SendTo.Server)]
    public void HitscanFireServerRpc(Vector3 headPos, Vector3 aimDir, RpcParams rpcParams = default)
    {
        if (!IsReady)
            return;

        if (NeedsReload)
            return;
        // Find out which client sent the request, so we can get the camera transform.
        Player player = GetPlayer(rpcParams.Receive.SenderClientId);
        if (!player)
            return;
        // Get the weapon data from the sender client
        WeaponData data = WeaponDatabase.GetWeapon(weaponID.Value);
        Vector3 dir = SpreadRandomizer(aimDir, data.bulletSpread);
        Vector3 adjustedHeadPos = headPos + (dir * 0.75f);
        // Send a raycast from the head to the new dirction within 100 units on only specific layers, and ignoring triggers
        if (Physics.Raycast(adjustedHeadPos, dir, out RaycastHit hit, 100f, Layer.BulletSurfaces, QueryTriggerInteraction.Ignore))
        {
            // If the raycast hit a player, damage the entity. For right now it just
            // damages the entity.
            if (hit.collider.CompareTag("Player"))
            {
                Entity e = hit.collider.GetComponent<Entity>();

                // Check if the entity is alive
                if (e && e.isAlive.Value)
                {
                    e.OnHit(new OnHitData()
                    {
                        attacker = player,
                        damage = data.damage,
                        sourceHit = hit.point,
                        movingDir = dir,
                        damageType = DamageType.Bullet
                    });
                }
            }
        }

        currentBullets.Value--;
        lastShootTime = Time.time;

        EffectsClientRpc(
            hit.point,
            currentBullets.Value,
            reserveBullets.Value,
            rpcParams.Receive.SenderClientId
        );
    }

    /// <summary>
    /// Every client's copy of this weapon handler will now create the vfx for their screens
    /// </summary>
    /// <param name="hitPoint"></param>
    /// <param name="currentBullets"></param>
    /// <param name="reserveBullets"></param>
    /// <param name="clientId"></param>
    [Rpc(SendTo.ClientsAndHost)]
    public void EffectsClientRpc(Vector3 hitPoint, int currentBullets, int reserveBullets, ulong clientId)
    {
        WeaponData data = WeaponDatabase.GetWeapon(weaponID.Value);

        // Create the bullet trail
        BulletTrail(
            data as HitscanData,
            firingPoint.position,
            hitPoint
        );

        // If the current client wasn't the client that requested the serverrpc, return
        if (NetworkManager.Singleton.LocalClientId != clientId)
            return;

        // Update the hud for the client that shot the weapon
        Player player = GetPlayer(clientId);

        if (!player)
            return;

        player.combat.UpdateWeaponInfo(
            data.weaponName,
            $"{currentBullets} / {reserveBullets}"
        );
    }

    /// <summary>
    /// Creates a trail from the firing point to the hit point.
    /// It also instantiates an object when it hits the point.
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    public void BulletTrail(HitscanData data, Vector3 startPos, Vector3 endPos)
    {
        Instantiate(data.bulletImpact, endPos, Quaternion.identity);
    }


    /// <summary>
    /// Randomizes the spread of the gun.
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="spread"></param>
    /// <returns></returns>
    public Vector3 SpreadRandomizer(Vector3 dir, float spread)
    {
        dir += new Vector3(
            UnityEngine.Random.Range(-spread, spread),
            UnityEngine.Random.Range(-spread, spread),
            UnityEngine.Random.Range(-spread, spread)
        );

        return dir.normalized;
    }

    /// <summary>
    /// [Called from Client]
    /// The server creates a WeaponDrop that all players can see and pick up.
    /// It also gets the client who threw it so that the WeaponDrop is thrown in the
    /// direction the client was facing.
    /// </summary>
    /// <param name="rpcParams"></param>
    [Rpc(SendTo.Server)]
    public void DropServerRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            Transform cam = client.PlayerObject.GetComponent<Player>().look.cam.transform;

            WeaponDrop drop = Instantiate(
                WeaponDatabase.TemplateDrop,
                cam.position,
                cam.rotation
            );
            
            drop.Init(weaponID.Value, true);
            drop.CreateWeapon(currentBullets.Value, reserveBullets.Value, cam.rotation);

            drop.NetworkObject.Spawn();
        }
    }
}
