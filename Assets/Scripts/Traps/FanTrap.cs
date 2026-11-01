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
    void Start()
    {
        StartCoroutine(WaitForCombat());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator OnOff()
    {
        while (GameManager.GamePhase == GamePhase.Combat)
        {
            //Turn on and wait
            windBox.enabled = true;
            windParticles.Play();
            Debug.Log("Fan On");
            yield return new WaitForSeconds(onTime);

            //Turn off and wait
            windBox.enabled = false;
            windParticles.Clear();
            windParticles.Pause();
            Debug.Log("Fan Off");
            yield return new WaitForSeconds(offTime);
        }
        StartCoroutine(WaitForCombat());
    }

    private IEnumerator WaitForCombat()
    {
        windParticles.Play();
        windBox.enabled = false;
        Debug.Log($"{this.name} is waiting for combat");
        yield return new WaitUntil(() => { return GameManager.GamePhase == GamePhase.Combat; });
        Debug.Log($"Combat has started {this.name} will turn on and off now.");
        StartCoroutine(OnOff());
    }
}
