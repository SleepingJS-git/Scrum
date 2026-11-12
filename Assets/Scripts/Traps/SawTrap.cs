using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class SawTrap : BreakableStructureEntity
{
    [SerializeField] private Collider damageCollider;
    [SerializeField] private GameObject blade;
    public int damage;
    private OnHitData hitData;
    private AudioSource audioPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hitData = new()
        {
            attacker = this,
            damage = damage,
            sourceHit = this.transform.position
        };
        audioPlayer = GetComponent<AudioSource>();
        StartCoroutine(WaitForCombat());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void ResetBreakable()
    {       
        base.ResetBreakable();
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
        if (other.CompareTag("Player")) {
            other.GetComponent<Player>().OnHit(hitData);
            if (audioPlayer.isPlaying) return;
            //audioPlayer.Play();
        }  
    }
}
