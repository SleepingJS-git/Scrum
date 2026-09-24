using System;
using System.Collections;
using UnityEngine;
[RequireComponent(typeof(Fracture))]
public class BreakableStructure: Entity
{
    private Fracture fracture;
    private Action fractureApart;
    [SerializeField] private float explosionForce, explosionRadius, upwardsModifier;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fracture = GetComponent<Fracture>();
        fractureApart = new Action(() => {
            fracture.CauseFracture();
            Explosion();
            StartCoroutine(Despawn());
            });
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
    
    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(5f);
        Destroy(fracture.FragmentRoot);
        Destroy(gameObject);
    }
}
