using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class FanTrap : BreakableStructureEntity
{
    [Tooltip("The time before the fan turns off.")]
    [SerializeField] private float offTime;
    [Tooltip("The time before the fan turns on.")]
    [SerializeField] private float onTime;
    [SerializeField] private Collider windBox;
    [SerializeField] private Collider fanCollider;
    [SerializeField] private ParticleSystem windParticles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    { 
        windBox.isTrigger = true;
        StartCoroutine(WaitForCombat()); 
    }

    private IEnumerator OnOff()
    {
        while (GameManager.GamePhase == GamePhase.Combat && isAlive.Value)
        {
            //Turn on and wait
            TurnOnRpc();
            yield return new WaitForSeconds(onTime);

            //Turn off and wait
            TurnOffRpc();
            yield return new WaitForSeconds(offTime);
        }
        StartCoroutine(WaitForCombat());
    }

    private IEnumerator WaitForCombat()
    {
        yield return new WaitUntil(() => { return GameManager.GamePhase == GamePhase.Building; });
        windParticles.Play();
        windBox.enabled = false;
        yield return new WaitUntil(() => { return GameManager.GamePhase == GamePhase.Combat; });
        StartCoroutine(OnOff());
    }

    public override void ResetBreakable()
    {
        fanCollider.enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
        if (!IsServer) return;
        this.health.Value = maxHealth;
        this.isAlive.Value = true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TurnOnRpc()
    {
        windBox.enabled = true;
        windParticles.Play();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TurnOffRpc()
    {
        windBox.enabled = false;
        windParticles.Clear();
        windParticles.Pause();
    }

    protected override void FractureApart()
    {
        windParticles.Clear();
        windParticles.Pause();
        fanCollider.enabled = false;
        base.FractureApart();
    }
}
