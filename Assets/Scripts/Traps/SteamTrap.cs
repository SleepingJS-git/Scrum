using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;

public class SteamTrap : BreakableStructureEntity
{
    [Tooltip("The time before the steam turns off.")]
    [SerializeField] private float offTime;
    [Tooltip("The time before the steam turns on.")]
    [SerializeField] private float onTime;
    [SerializeField] private int damage;

    [SerializeField] private Collider damageCollider;
    [SerializeField] private ParticleSystem steamParticles;

    private OnHitData hitData;
    private Coroutine toggleRoutine;

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

    public override void ResetBreakable()
    {
        steamParticles.Play();
        base.ResetBreakable();
        StartCoroutine(WaitForCombat());
    }

    protected override void FractureApart()
    {
        StopCoroutine(toggleRoutine);
        steamParticles.Pause();
        steamParticles.Clear();
        damageCollider.enabled = false;
        base.FractureApart();
    }

    private IEnumerator OnOff()
    {
        while (!GameManager.Instance || GameManager.GamePhase == GamePhase.Combat)
        {
            damageCollider.enabled = true;
            steamParticles.Play();
            yield return new WaitForSeconds(onTime);

            steamParticles.Clear();
            steamParticles.Pause();
            damageCollider.enabled = false;
            yield return new WaitForSeconds(offTime);
        }
        StartCoroutine(WaitForCombat());
    }

    private IEnumerator WaitForCombat()
    {
        steamParticles.Play();
        damageCollider.enabled = false;
        yield return new WaitUntil(() => { return GameManager.GamePhase == GamePhase.Combat; });
        toggleRoutine = StartCoroutine(OnOff());
    }
    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;
        RaycastHit hit;
        if (other.CompareTag("Player"))
        {
            Physics.Raycast(this.transform.position, other.transform.position - this.transform.position, out hit, Layer.Player | Layer.Buildable | Layer.Ground | Layer.Wall);
            if (!hit.collider.CompareTag("Player")) return;
            other.GetComponent<Player>().OnHit(hitData); 
        }
    }
}
