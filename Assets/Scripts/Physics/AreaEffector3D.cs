using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class AreaEffector3D : MonoBehaviour
{   
    [Tooltip("Direction the force is applied in.")]
    [SerializeField] private Vector3 forceDir;
    [Tooltip("Amount of force to apply.")]
    [SerializeField] private float force;
    [Tooltip("The collider that will apply the force, will make the selected collider a trigger.")]
    [SerializeField] private Collider effectArea;

    // Start is called before the first frame update
    void Start()
    {
        effectArea.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody areaEffectee;
        CharacterController characterEffected;
        
        if(other.gameObject.TryGetComponent<Rigidbody>(out areaEffectee))
        {
            areaEffectee.AddForceAtPosition(forceDir * force, other.ClosestPoint(this.transform.position));
        }
        else if (other.gameObject.TryGetComponent<CharacterController>(out characterEffected))
        {
            characterEffected.SimpleMove(forceDir * force);
        }
    }
}
