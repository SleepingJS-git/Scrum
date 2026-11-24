using System.Collections;
using UnityEngine;

public class SawTrap : BreakableStructureEntity
{
    [SerializeField] private Collider damageCollider;
    [SerializeField] private GameObject blade;
    public int damage;
    private OnHitData hitData;

    [SerializeField] private GameAudioData audioData;
    [SerializeField] private AudioSource humSource;
    [SerializeField] private AudioSource hitSource;

    private float hitVolume;
    private float lastHitTime = float.NegativeInfinity;

    void Start()
    {
        hitData = new()
        {
            attacker = this,
            damage = damage,
            sourceHit = transform.position
        };

        hitVolume = hitSource.volume;
        hitSource.clip = audioData.sawTrapHit;
        hitSource.loop = true;
        hitSource.playOnAwake = false;
        hitSource.Stop();
        hitSource.volume = 0f;

        StartCoroutine(WaitForCombat());
    }

    //Math for getting the cutting sound fading in and out
    void Update()
    {
        if (!IsServer) return;

        bool hitting = damageCollider.enabled && blade.activeInHierarchy && Time.time - lastHitTime < 0.1f;

        if (hitting && !hitSource.isPlaying)
            hitSource.Play();

        float targetVolume = hitting ? hitVolume : 0f;
        float fadeTime = hitting ? 0.08f : 0.1f;

        hitSource.volume = Mathf.MoveTowards(hitSource.volume, targetVolume, hitVolume * Time.deltaTime / fadeTime);

        if (!hitting && hitSource.volume <= 0f && hitSource.isPlaying)
            hitSource.Stop();
    }

    public override void ResetBreakable()
    {
        base.ResetBreakable();
        lastHitTime = float.NegativeInfinity;
        blade.SetActive(true);
    }

    protected override void FractureApart()
    {
        blade.SetActive(false);
        damageCollider.enabled = false;
        base.FractureApart();
    }

    private IEnumerator WaitForCombat()
    {
        yield return new WaitUntil(() => GameManager.GamePhase == GamePhase.Combat);
        damageCollider.enabled = true;
        StartCoroutine(WaitForCombatEnd());
    }

    private IEnumerator WaitForCombatEnd()
    {
        yield return new WaitUntil(() => GameManager.GamePhase == GamePhase.EndOfCombat);
        damageCollider.enabled = false;
        StartCoroutine(WaitForCombat());
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;
        if (!damageCollider.enabled) return;

        if (other.CompareTag("Player"))
        {
            lastHitTime = Time.time;
            other.GetComponent<Player>().OnHit(hitData);
        }
    }
}