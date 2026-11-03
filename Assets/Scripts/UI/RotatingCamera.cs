using UnityEngine;


public class RotatingCamera : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private float spinSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        mainCamera.transform.Rotate(Vector3.up, spinSpeed * Time.unscaledDeltaTime, Space.Self);
    }
}
