using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class SawTrap : Entity
{
    [SerializeField] private Collider damageCollider;
    public int damage;
    private OnHitData hitData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hitData = new()
        {
            attacker = this,
            damage = damage,
            sourceHit = this.transform.position
        };
        StartCoroutine(WaitForCombat());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator WaitForCombat()
    {
        yield return new WaitUntil(() => { return GameManager.GamePhase == GamePhase.Combat;});
        damageCollider.enabled = true;
        StartCoroutine(WaitForCombatEnd());
    }

    private IEnumerator WaitForCombatEnd()
    {
        yield return new WaitUntil(() => { return GameManager.GamePhase == GamePhase.EndOfCombat; });
        damageCollider.enabled = false;
        StartCoroutine(WaitForCombat());
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;
        Debug.Log("Hurting Player");
        if (other.CompareTag("Player")) {other.GetComponent<Player>().OnHit(hitData);}  
    }
}
