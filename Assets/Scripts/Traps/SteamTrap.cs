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
    [SerializeField] private AudioSource humSource;
    [SerializeField] private AudioSource hitSource;

    private OnHitData hitData;
    private Coroutine toggleRoutine;
    private Coroutine combatEnd;

    private float humVolume;
    private float hitVolume;
    private float lastHitTime = float.NegativeInfinity;

    void Start()
    {
        humVolume = humSource.volume;
        humSource.volume = 0f;

        hitVolume = hitSource.volume;
        hitSource.loop = true;
        hitSource.playOnAwake = false;
        hitSource.Stop();
        hitSource.volume = 0f;

        hitData = new()
        {
            attacker = this,
            damage = damage,
            sourceHit = transform.position
        };

        StartCoroutine(WaitForCombat());
    }

    void Update()
    {
        bool steaming = damageCollider.enabled;

        float humTarget = steaming ? humVolume : 0f;
        humSource.volume = Mathf.MoveTowards(humSource.volume, humTarget, humVolume * Time.deltaTime / 0.15f);

        bool hitting = steaming && IsServer && Time.time - lastHitTime < 0.1f;

        if (hitting && !hitSource.isPlaying)
            hitSource.Play();

        float hitTarget = hitting ? hitVolume : 0f;
        float fadeTime = hitting ? 0.08f : 0.1f;
        hitSource.volume = Mathf.MoveTowards(hitSource.volume, hitTarget, hitVolume * Time.deltaTime / fadeTime);

        if (!hitting && hitSource.volume <= 0f && hitSource.isPlaying)
            hitSource.Stop();
    }

    public override void ResetBreakable()
    {
        lastHitTime = float.NegativeInfinity;
        steamParticles.Play();
        base.ResetBreakable();
        StartCoroutine(WaitForCombat());
    }

    protected override void FractureApart()
    {
        if (toggleRoutine != null)
            StopCoroutine(toggleRoutine);

        if (combatEnd != null)
            StopCoroutine(combatEnd);

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
    }

    private IEnumerator WaitForCombat()
    {
        lastHitTime = float.NegativeInfinity;
        steamParticles.Play();
        damageCollider.enabled = false;
        Debug.Log("SteamTrap waiting for combat.");

        yield return new WaitUntil(() => GameManager.GamePhase == GamePhase.Combat);

        toggleRoutine = StartCoroutine(OnOff());
        combatEnd = StartCoroutine(WaitForCombatEnd());
    }

    private IEnumerator WaitForCombatEnd()
    {
        yield return new WaitUntil(() => GameManager.GamePhase == GamePhase.EndOfCombat);

        if (toggleRoutine != null)
            StopCoroutine(toggleRoutine);

        damageCollider.enabled = false;
        StartCoroutine(WaitForCombat());
        Debug.Log("Combat has ended.");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer || !damageCollider.enabled) return;

        if (other.CompareTag("Player"))
        {
            bool blocked = !Physics.Raycast(transform.position, other.transform.position - transform.position,
                out RaycastHit hit, Mathf.Infinity, Layer.Player | Layer.Buildable | Layer.Ground | Layer.Wall);

            if (blocked || !hit.collider.CompareTag("Player")) return;

            lastHitTime = Time.time;
            other.GetComponent<Player>().OnHit(hitData);
        }
    }
}