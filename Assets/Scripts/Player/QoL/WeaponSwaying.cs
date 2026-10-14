using UnityEngine;

public class WeaponSwaying : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Player main;
    [SerializeField] private HeadBobbing bob;

    [Header("Tuneables")]
    [SerializeField] private float step = 0.01f;
    [SerializeField] private float maxStepDistance = 0.06f;
    [SerializeField] private float smooth = 10f;
    [SerializeField] private float stepRot = 0.01f;
    [SerializeField] private float maxRotStep = 0.06f;
    [SerializeField] private float smoothRot = 12f;

    private Vector3 _swayPos;
    private Vector3 _swayRot;
    private bool _enabled = true;

    void Start()
    {
        _enabled = main.IsOwner;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_enabled) return;
        CompositeSway();

        Sway();
        SwayRotation();
    }

    private void CompositeSway()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            _swayPos + (bob.bobMotion / 4f),
            Time.deltaTime * smooth
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            Quaternion.Euler(_swayRot),
            Time.deltaTime * smoothRot
        );
    }

    private void Sway()
{
    Vector3 invertLook = main.LookInput * (-step);

    invertLook.x = Mathf.Clamp(
        invertLook.x,
        -maxStepDistance,
        maxStepDistance
    );

    invertLook.y = Mathf.Clamp(
        invertLook.y,
        -maxStepDistance / 2f,
        maxStepDistance / 2f
    );

    _swayPos = invertLook;
}

    private void SwayRotation()
    {
        Vector3 invertLook = main.LookInput * (-stepRot);

        invertLook.x = Mathf.Clamp(
            invertLook.x,
            -maxRotStep,
            maxRotStep
        );

        invertLook.y = Mathf.Clamp(
            invertLook.y,
            -maxRotStep,
            maxRotStep
        );

        _swayRot = new Vector3(
            invertLook.y,
            invertLook.x,
            invertLook.x
        );
    }
}
