using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
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
    [SerializeField] private List<GameObject> affectedPlayers;

    // Start is called before the first frame update
    void Start()
    {
        effectArea.isTrigger = true;

        affectedPlayers = new List<GameObject>();
    }

    public void ClearAffected()
    {
        for (int i = affectedPlayers.Count - 1;  i >= 0; i--)
        {
            Vector3 dir = useForwardTransform ? transform.forward : forceDir;
            Player p = affectedPlayers[i].GetComponent<Player>();
            p.ApplyContinuousForceRpc(-dir, force, RpcTarget.Single(p.OwnerClientId, RpcTargetUse.Temp));

            affectedPlayers.RemoveAt(i);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (!other.CompareTag("Player")) return;

        if (!affectedPlayers.Contains(other.gameObject))
            affectedPlayers.Add(other.gameObject);
        else return;

        Debug.Log("Player is affected: " + other.name);
        
        Vector3 dir = useForwardTransform ? transform.forward : forceDir;

        Player p = other.GetComponent<Player>();
        p.ApplyContinuousForceRpc(dir, force, RpcTarget.Single(p.OwnerClientId, RpcTargetUse.Temp));
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (!other.CompareTag("Player")) return;

        if (affectedPlayers.Contains(other.gameObject))
            affectedPlayers.Remove(other.gameObject);
        else return;

        Debug.Log("Player is affected: " + other.name);

        Vector3 dir = useForwardTransform ? transform.forward : forceDir;

        Player p = other.GetComponent<Player>();
        p.ApplyContinuousForceRpc(-dir, force, RpcTarget.Single(p.OwnerClientId, RpcTargetUse.Temp));
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Player")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (!rb) return;

        Vector3 dir = useForwardTransform ? transform.forward : forceDir;

        if (Physics.Linecast(other.transform.position, transform.position + transform.forward * 0.3f, Layer.Buildable | Layer.Wall | Layer.Ground, QueryTriggerInteraction.Ignore)) return;

        Vector3 toFanDir = transform.position - other.transform.position;

        float dist = toFanDir.magnitude;

        if (dist > maxDist) return;
        float norm = dist / maxDist;

        float newForce = Mathf.Lerp(maxForce, force, norm);


        rb.AddForceAtPosition(dir * newForce, other.ClosestPoint(transform.position));
    }
}
