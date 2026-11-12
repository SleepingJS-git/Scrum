using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class SlowTrap : NetworkBehaviour
{
    [Tooltip("The amount of time the slow should last once the player steps off.")]
    [SerializeField] private float slowTimer;

    [Tooltip("Speed that the player's speed should be reset to.")]
    [SerializeField] private float speedMultiplier;
    Coroutine toggleRoutine;
    private Dictionary<GameObject, Coroutine> playersSlowed = new Dictionary<GameObject, Coroutine>();

    private IEnumerator ToggleSlow(Player targetPlayer)
    {
        yield return new WaitForSeconds(slowTimer);
        targetPlayer.ApplySpeedMultiplierRpc(1.0f, RpcTarget.Single(targetPlayer.OwnerClientId, RpcTargetUse.Temp));
        Debug.Log("PlayerSpeed Reset");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;
        if (!other.CompareTag("Player")) return;
        Player targetPlayer = other.GetComponent<Player>();
        targetPlayer.ApplySpeedMultiplierRpc(speedMultiplier, RpcTarget.Single(targetPlayer.OwnerClientId, RpcTargetUse.Temp));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        if (!other.CompareTag("Player")) return;
        Player p = other.GetComponent<Player>();
        if (playersSlowed.ContainsKey(p.gameObject)) 
        {
            StopCoroutine(playersSlowed[p.gameObject]);
            playersSlowed.Remove(p.gameObject);
            playersSlowed.Add(p.gameObject, StartCoroutine(ToggleSlow(p)));
        }
        else { playersSlowed.Add(p.gameObject, StartCoroutine(ToggleSlow(p))); }
    }
}
