using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Netcode;
using UnityEngine;


public class AreaEffector3D : NetworkBehaviour
{   
    [Tooltip("Direction the force is applied in.")]
    [SerializeField] private Vector3 forceDir;
    [Tooltip("Amount of force to apply.")]
    [SerializeField] private float force;
    [SerializeField] private float maxForce;
    [SerializeField] private float maxDist;
    [Tooltip("The collider that will apply the force, will make the selected collider a trigger.")]
    [SerializeField] private Collider effectArea;
    [Tooltip("Whether or not to use this objects forward transform as the force direction.")]
    [SerializeField] private bool useForwardTransform;

    // Start is called before the first frame update
    void Start()
    {
        effectArea.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;

        Vector3 dir = useForwardTransform ? transform.forward : forceDir;

        Vector3 toFanDir = transform.position - other.transform.position;
        float dist = toFanDir.magnitude;

        if (dist > maxDist) return;
        float norm = dist / maxDist;

        float newForce = Mathf.Lerp(maxForce, force, norm);

        if (other.CompareTag("Player"))
        {
            Player p = other.GetComponent<Player>();
            p.AddForceRpc(dir, newForce, 0, RpcTarget.Single(p.OwnerClientId, RpcTargetUse.Temp));
        }
        else
        {
            Rigidbody rb = other.attachedRigidbody;
            if (!rb) return;
            rb.AddForceAtPosition(dir * newForce, other.ClosestPoint(transform.position));
        }
    }
}
