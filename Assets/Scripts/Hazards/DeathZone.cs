using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeathZone : NetworkBehaviour
{
    private OnHitData onHitData;
    [SerializeField] private float explosionForce;
    private List<Entity> cachedEntities;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        cachedEntities = new List<Entity>();
        onHitData = new OnHitData()
        {
            damage = 9999,
            damageType = DamageType.DeathZone,
            explosionForce = explosionForce,
            radius = 1f,
            upwardsModifier = 2f,
            movingDir = Vector3.up
        };

        PlayerManager.OnAllPlayersReset += RemoveCachedPlayers;
    }

    private void RemoveCachedPlayers()
    {
        cachedEntities.Clear();
    }
    void OnTriggerEnter(Collider collider)
    {
        if (collider)
        {
            Entity e = collider.GetComponent<Entity>();
            if (e) DeathZoneRpc(e.NetworkObjectId);
        }
    }

    [Rpc(SendTo.Server)]
    private void DeathZoneRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject obj))
        {
            Entity e = obj.GetComponent<Entity>();
            
            if (e && e.isAlive.Value)
            {
                Vector3 pos = e.transform.position; pos.y += -1.5f;
                onHitData.sourceHit = pos;

                e.OnHit(onHitData);
                
                cachedEntities.Add(e);
            }
        }
    }
}
