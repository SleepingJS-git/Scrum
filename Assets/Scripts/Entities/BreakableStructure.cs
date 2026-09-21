using System;
using System.Collections;
using UnityEngine;
[RequireComponent(typeof(Fracture))]
public class BreakableStructure: Entity
{
    private Fracture fracture;
    private Action fractureApart;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fracture = GetComponent<Fracture>();
        fractureApart = new Action(() => {
            fracture.CauseFracture();
            StartCoroutine(Despawn());
            });
        AddDeathEvent(fractureApart);
    }

    // Update is called once per frame
    void Update()
    {
        
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
