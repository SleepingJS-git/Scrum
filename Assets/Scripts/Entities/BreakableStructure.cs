using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
[RequireComponent(typeof(Fracture))]
public class BreakableStructure: Entity
{
    private Fracture fracture;
    private Action fractureApart;
    [SerializeField] private float explosionForce, explosionRadius, upwardsModifier;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        fracture = GetComponent<Fracture>();
        fractureApart = new Action(FractureApartClientRpc);
        AddDeathEvent(fractureApart);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Explosion()
    {
        foreach(Rigidbody rb in fracture.FragmentRoot.GetComponentsInChildren<Rigidbody>())
        {
            rb.AddExplosionForce(
                explosionForce, 
                transform.position, explosionRadius, upwardsModifier,
                ForceMode.Impulse);
        }
    }

    public override void OnHit(OnHitData onHitData)
    {
        base.OnHit(onHitData);
    }

    private IEnumerator LoseHealth()
    {
        while (health.Value > 0)
        {
            health.Value--;
            yield return new WaitForSeconds(1f);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void FractureApartClientRpc()
    {
        fracture.CauseFracture();
        Explosion();
        StartCoroutine(Despawn());
    }


    
    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(5f);
        Destroy(fracture.FragmentRoot);
        gameObject.SetActive(false);
    }
}
