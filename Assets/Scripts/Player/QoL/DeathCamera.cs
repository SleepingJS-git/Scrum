using System.Collections;
using UnityEngine;

public class DeathCamera : MonoBehaviour
{
    [SerializeField] private GameObject camObj;
    [SerializeField] private Transform followTransform;
    private bool _enabled = false;
    public void Init()
    {
        // PlayerManager.ToBuilding += () =>
        // {
        //     Toggle(false);
        // };
    }
    void Update()
    {
        if (!_enabled) return;

        camObj.transform.LookAt(followTransform);
    }

    public void Toggle(bool isOn)
    {
        camObj.SetActive(isOn);
        _enabled = isOn;
    }

    public void SwapToBuildCamTimer()
    {
        if (PlayerManager.BuildingUI)
            StartCoroutine(SwapToBuildCamRoutine());
    }

    private IEnumerator SwapToBuildCamRoutine()
    {
        yield return new WaitForSeconds(4.95f);
        PlayerManager.Instance.ChangePov(1);
        PlayerManager.BuildingUI.gameObject.SetActive(false);
    }
}
