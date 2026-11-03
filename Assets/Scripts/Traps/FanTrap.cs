using System.Collections;
using UnityEngine;

public class FanTrap : MonoBehaviour
{
    [Tooltip("The time before the fan turns off.")]
    [SerializeField] private float offTime;
    [Tooltip("The time before the fan turns on.")]
    [SerializeField] private float onTime;
    [SerializeField] private Collider windBox;
    [SerializeField] private ParticleSystem windParticles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { StartCoroutine(WaitForCombat()); }

    private IEnumerator OnOff()
    {
        while (GameManager.GamePhase == GamePhase.Combat)
        {
            //Turn on and wait
            windBox.enabled = true;
            windParticles.Play();
            yield return new WaitForSeconds(onTime);

            //Turn off and wait
            windBox.enabled = false;
            windParticles.Clear();
            windParticles.Pause();
            yield return new WaitForSeconds(offTime);
        }
        StartCoroutine(WaitForCombat());
    }

    private IEnumerator WaitForCombat()
    {
        windParticles.Play();
        windBox.enabled = false;
        yield return new WaitUntil(() => { return GameManager.GamePhase == GamePhase.Combat; });
        StartCoroutine(OnOff());
    }
}
