using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeathZone : NetworkBehaviour
{
    private OnHitData onHitData;
    [SerializeField] private float explosionForce;
    private bool _enabled = true;
    private Dictionary<Collider, float> kvp;
    public override void OnNetworkSpawn()
    {
        onHitData = new OnHitData()
        {
            damage = 9999,
            damageType = DamageType.DeathZone,
            explosionForce = explosionForce,
            radius = 1f,
            upwardsModifier = 2f,
            movingDir = Vector3.up
        };
        if (!IsServer) return;
        kvp = new Dictionary<Collider, float>();

        PlayerManager.ToBuilding += () =>
        {
            _enabled = false;
            kvp.Clear();
        };
        PlayerManager.ToCombat += () =>
        {
            _enabled = true;
            kvp.Clear();
        };
    }

    void OnTriggerStay(Collider collider)
    {
        if (!IsServer) return;
        if (!_enabled) return;
        if (collider)
        {
            if (!kvp.ContainsKey(collider))
            {
                kvp[collider] = 0f;
                return;
            }

            if (kvp[collider] < 0f)
                return;

            kvp[collider] += Time.deltaTime;

            if (kvp[collider] >= .2f)
            {
                ProcessDeath(collider.GetComponent<Entity>());
                kvp[collider] = -1f;
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (!IsServer) return;
        if (collider)
        {
            if (kvp.ContainsKey(collider))
            {
                kvp.Remove(collider);
            }
        }
    }

    private void ProcessDeath(Entity e)
    {
        if (e && e.isAlive.Value)
        {
            Vector3 pos = e.transform.position; pos.y += -1.5f;
            onHitData.sourceHit = pos;

            e.OnHit(onHitData);
        }
    }
}
