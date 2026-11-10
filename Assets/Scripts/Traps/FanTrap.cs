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
    private Coroutine toggleRoutine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
        AddDeathEvent(() => areaEffector3D.ClearAffected());
        StartCoroutine(WaitForCombat());
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
            //Turn on and wait
            windBox.enabled = true;
            windParticles.Play();
            yield return new WaitForSeconds(onTime);

            //Turn off and wait
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
        yield return new WaitUntil(() => { return GameManager.GamePhase == GamePhase.Combat; });
        toggleRoutine = StartCoroutine(OnOff());
    }
}
