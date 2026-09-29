using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
[RequireComponent(typeof(Fracture))]
[RequireComponent(typeof(AudioSource))]
public class ExplosiveStructure : BreakableStructureEntity
{
    public int damage;
    private AudioSource explosionAudioSource;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        fracture = GetComponent<Fracture>();
        explosionAudioSource = GetComponent<AudioSource>();
        fractureApart = new Action(FractureApartClientRpc);
        AddDeathEvent(fractureApart);
        AddDeathEvent(Explode);
    }

    private void Explode()
    {
            PlayExplosionSoundRpc();

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

    [Rpc(SendTo.Everyone)]
    private void PlayExplosionSoundRpc()
    {
        StartCoroutine(PlayExplosionSoundDelayed());
    }

    //very temp fix, do not want barrels all playing on top of each other too loud
    private IEnumerator PlayExplosionSoundDelayed()
    {
        float delay = UnityEngine.Random.Range(0f, 0.5f);
        yield return new WaitForSeconds(delay);
        explosionAudioSource.Play();
    }
}
