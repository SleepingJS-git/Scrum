using System;
using UnityEngine;
[RequireComponent(typeof(Fracture))]
public class BreakableStructure: Entity
{
    private Fracture fractue;
    private Action fractureApart;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fractue = GetComponent<Fracture>();
        fractureApart = new Action(() => fractue.CauseFracture());
        AddDeathEvent(fractureApart);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
