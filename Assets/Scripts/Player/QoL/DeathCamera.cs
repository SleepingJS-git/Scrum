using System.Collections;
using UnityEngine;

public class DeathCamera : MonoBehaviour
{
    [SerializeField] private GameObject camObj;
    private Transform followTransform;
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
        
        if (followTransform)
            camObj.transform.LookAt(followTransform);
    }
    public void SetTarget(Transform target)
    {
        followTransform = target;
    }
    public void Toggle(bool isOn)
    {
        camObj.SetActive(isOn);
        _enabled = isOn;
    }

    public void SwapToBuildCamTimer()
    {
        if (!PlayerManager.BuildingUI) return;

        if (PlayerManager.IsHostClient)
        {
            if (PlayerManager.ConfigType == ConfigType.Full_Game)
                StartCoroutine(SwapToBuildCamRoutine());
        }
        else
            StartCoroutine(SwapToBuildCamRoutine());
    }

    private IEnumerator SwapToBuildCamRoutine()
    {
        yield return new WaitForSeconds(3f);
        PlayerManager.Instance.ChangePov(1);
        PlayerManager.BuildingUI.gameObject.SetActive(false);
    }
}
