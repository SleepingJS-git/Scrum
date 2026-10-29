using System.Collections;
using UnityEngine;

public class FanTrap : MonoBehaviour
{
    [Tooltip("The time this waits before turning on or off.")]
    [SerializeField] private float timer;
    [SerializeField] private Collider windBox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(OnOff());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator OnOff()
    {
        while (true)
        {
            windBox.enabled = !windBox.enabled;
            yield return new WaitForSeconds(timer);
        }
    }
}
