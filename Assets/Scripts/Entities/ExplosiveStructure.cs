using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
[RequireComponent(typeof(Fracture))]
public class ExplosiveStructure : BreakableStructureEntity
{
    public int damage;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        fracture = GetComponent<Fracture>();
        fractureApart = new Action(FractureApartClientRpc);
        AddDeathEvent(fractureApart);
        AddDeathEvent(Explode);
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, Layer.ExplosionEffected);
        OnHitData onHitData = new()
        {
            attacker = this, damage = damage, sourceHit = transform.position,
            radius = explosionRadius, explosionForce = explosionForce,
            upwardsModifier = upwardsModifier, damageType = DamageType.Explosion
        };
        foreach (Collider h in hits)
        {
            if (h == null || h.gameObject == gameObject) continue;
            if (h.attachedRigidbody)
            {
                h.attachedRigidbody.AddExplosionForce(
                    onHitData.damage * 10f, 
                    onHitData.sourceHit, 2f, .5f,
                    ForceMode.Impulse);
            }

            Entity e = h.GetComponent<Entity>();
            if (e && e.isAlive.Value)
                e.OnHit(onHitData);
        }
    }
}
