using System.Collections;
using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Tuneables")]
    public float recoilStep;
    public float initialRecoilTime;
    public float recoilResetTime;
    private float defaultResetTime;
    private Quaternion startingRotation;
    private Coroutine recoilRoutine;
    void Start()
    {
        startingRotation = transform.localRotation;
        defaultResetTime = recoilResetTime;
    }
    public void SetResetTime(float time = 0f)
    {
        if (time == 0f)
            recoilResetTime = defaultResetTime;
        else recoilResetTime = time;
    }
    public void AddRecoil(float recoilAmplitude)
    {
        if (recoilRoutine != null) 
            StopCoroutine(recoilRoutine);

        recoilRoutine = StartCoroutine(RecoilRoutine(recoilAmplitude));
    }

    private IEnumerator RecoilRoutine(float recoilAmplitude)
    {
        Quaternion startRotation = transform.localRotation;

        Vector3 targetEuler = startRotation.eulerAngles;
        targetEuler.x -= recoilStep * recoilAmplitude;

        Quaternion recoilRotation = Quaternion.Euler(targetEuler);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / initialRecoilTime;

            transform.localRotation = Quaternion.Lerp(
                startRotation,
                recoilRotation,
                t
            );

            yield return null;
        }

        t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / recoilResetTime;

            transform.localRotation = Quaternion.Lerp(
                recoilRotation,
                startingRotation,
                t
            );

            yield return null;
        }

        transform.localRotation = startingRotation;
        recoilRoutine = null;
    }
}
