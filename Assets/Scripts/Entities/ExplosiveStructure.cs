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
    [Tooltip("The amount of time the explosive waits before exploding.")]
    [SerializeField] private float breakDelay;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        fracture = GetComponent<Fracture>();
        explosionAudioSource = GetComponent<AudioSource>();
        fractureApart = new Action(FractureApartClientRpc);
        AddDeathEvent(fractureApart);
        //AddDeathEvent(Explode);
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

    [Rpc(SendTo.Everyone)]
    private void PlayExplosionSoundRpc()
    {
        explosionAudioSource.Play();
    }

    private void Fracture()
    {
        foreach(Rigidbody rb in fracture.FragmentRoot.GetComponentsInChildren<Rigidbody>())
        {
            rb.AddExplosionForce(
                explosionForce, 
                transform.position, explosionRadius, upwardsModifier,
                ForceMode.Impulse);
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void FractureApartClientRpc()
    {
        StartCoroutine(DelayExplosion());
        //fracture.CauseFracture();
        //Fracture();
        StartCoroutine(Despawn());
    }

    private IEnumerator DelayExplosion()
    {
        PlayExplosionSoundRpc();
        yield return new WaitForSeconds(breakDelay);
        Explode();
        Fracture();
    }
    
    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(5f);
        Destroy(fracture.FragmentRoot);
        gameObject.SetActive(false);
    }
}
