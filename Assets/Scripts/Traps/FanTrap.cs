using System.Collections;
using UnityEngine;

public class FanTrap : BreakableStructureEntity
{
    [Tooltip("The time before the fan turns off.")]
    [SerializeField] private float offTime;
    [Tooltip("The time before the fan turns on.")]
    [SerializeField] private float onTime;
    [SerializeField] private Collider windBox;
    [SerializeField] private ParticleSystem windParticles;
    [SerializeField] private AreaEffector3D areaEffector3D;
    [SerializeField] private Collider fanCollider;
    [SerializeField] private AudioSource fanSource;

    private Coroutine toggleRoutine;
    private float fanVolume;

    void Start()
    {
        fanVolume = fanSource.volume;
        fanSource.volume = 0f;

        AddDeathEvent(() => areaEffector3D.ClearAffected());
        StartCoroutine(WaitForCombat());
    }

    void Update()
    {
        float targetVolume = windBox.enabled ? fanVolume : 0f;
        fanSource.volume = Mathf.MoveTowards(fanSource.volume, targetVolume, fanVolume * Time.deltaTime / 0.15f);
    }

    public override void ResetBreakable()
    {
        base.ResetBreakable();
        StartCoroutine(WaitForCombat());
    }

    protected override void FractureApart()
    {
        StopCoroutine(toggleRoutine);
        base.FractureApart();
        windParticles.Clear();
        windParticles.Pause();
        fanCollider.enabled = false;
        windBox.enabled = false;
    }

    private IEnumerator OnOff()
    {
        while (!GameManager.Instance || GameManager.GamePhase == GamePhase.Combat)
        {
            // Turn on and wait
            windBox.enabled = true;
            windParticles.Play();
            yield return new WaitForSeconds(onTime);

            // Turn off and wait
            windBox.enabled = false;
            windParticles.Clear();
            windParticles.Pause();
            areaEffector3D.ClearAffected();
            yield return new WaitForSeconds(offTime);
        }

        StartCoroutine(WaitForCombat());
    }

    private IEnumerator WaitForCombat()
    {
        windParticles.Play();
        windBox.enabled = false;
        yield return new WaitUntil(() => GameManager.GamePhase == GamePhase.Combat);
        toggleRoutine = StartCoroutine(OnOff());
    }
}