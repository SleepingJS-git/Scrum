using Unity.Netcode;
using UnityEngine;

public class WeaponSpawner : NetworkBehaviour
{
    public WeaponData data;
    public Transform holder;
    public WeaponDrop weaponDropTemplate; // This will be moved to a singleton manager.
    public float spawnTime;
    private float _dt;
    private WeaponDrop drop;
    public void SpawnWeapon()
    {
        drop = Instantiate(
            weaponDropTemplate,
            holder.position,
            holder.rotation
        );
        drop.Init(data.weaponID, false);
        drop.CreateWeapon(data);
        drop.NetworkObject.Spawn();
    }

    void Update()
    {
        if (!IsServer) return;

        if (drop) return;

        _dt -= Time.deltaTime;
        if (_dt > 0f) return;

        SpawnWeapon();
        _dt = spawnTime;
    }
}
