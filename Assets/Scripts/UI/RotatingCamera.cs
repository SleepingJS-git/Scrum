using UnityEngine;


public class RotatingCamera : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private float spinSpeed;

    void LateUpdate()
    {
        mainCamera.transform.Rotate(Vector3.up, spinSpeed * Time.unscaledDeltaTime, Space.Self);
    }
}
