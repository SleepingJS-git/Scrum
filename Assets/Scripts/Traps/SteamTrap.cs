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
    private Coroutine combatEnd;
    private AudioSource audioPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioPlayer = GetComponent<AudioSource>();
        hitData = new()
        {
            attacker = this,
            damage = damage,
            sourceHit = this.transform.position
        };

        StartCoroutine(WaitForCombat());
    }

    public override void ResetBreakable()
    {
        steamParticles.Play();
        base.ResetBreakable();
        StartCoroutine(WaitForCombat());
    }

    protected override void FractureApart()
    {
        audioPlayer.Stop();
        StopCoroutine(toggleRoutine);
        StopCoroutine(combatEnd);
        steamParticles.Pause();
        steamParticles.Clear();
        damageCollider.enabled = false;
        base.FractureApart();
    }

    private IEnumerator OnOff()
    {
        audioPlayer.Play();
        while (!GameManager.Instance || GameManager.GamePhase == GamePhase.Combat)
        {
            AudioFadeInOut(0.01f, true);
            
            damageCollider.enabled = true;
            steamParticles.Play();
            yield return new WaitForSeconds(onTime);

            AudioFadeInOut(0.05f, false);
            steamParticles.Clear();
            steamParticles.Pause();
            damageCollider.enabled = false;
            yield return new WaitForSeconds(offTime);
        }
        //StartCoroutine(WaitForCombat());
    }

    IEnumerator AudioFadeInOut(float speed, bool fade)
    {
        //True Fades in
        if(fade)
        {
            for (float i = 0; i < 1; i += speed)
            {
                audioPlayer.volume = i;
            }
        }
        else
        {
            for (float i = 1; i > 0; i -= speed)
            {
                audioPlayer.volume = i;
            }
        }
            yield return null;
    }

    private IEnumerator WaitForCombat()
    {
        audioPlayer.Stop();
        steamParticles.Play();
        damageCollider.enabled = false;
        Debug.Log("SteamTrap waiting for combat.");
        yield return new WaitUntil(() => { return GameManager.GamePhase == GamePhase.Combat; });
        toggleRoutine = StartCoroutine(OnOff());
        combatEnd = StartCoroutine(WaitForCombatEnd());
    }

    private IEnumerator WaitForCombatEnd()
    {
        yield return new WaitUntil(() => { return GameManager.GamePhase == GamePhase.EndOfCombat; });
        damageCollider.enabled = false;
        StartCoroutine(WaitForCombat());
        Debug.Log("Combat has ended.");
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
