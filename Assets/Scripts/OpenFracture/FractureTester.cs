using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractureTester : MonoBehaviour
{
    [SerializeField] private Fracture test;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(FractureOnTimer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private IEnumerator FractureOnTimer()
    {
       yield return new WaitForSeconds(5f);
        test.CauseFracture();
    }
}
