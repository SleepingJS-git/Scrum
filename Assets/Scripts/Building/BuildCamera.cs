using UnityEngine;
using UnityEngine.InputSystem;

public class BuildCamera : MonoBehaviour
{
    [SerializeField]
    private GridPlacement gridBuilding;
    private PlayerInput playerInput;
    private InputAction pan;
    private InputAction orbit;

    private float panSpeed = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = gridBuilding.GetComponent<PlayerInput>();
        pan = playerInput.actions.FindAction("Pan");
        orbit = playerInput.actions.FindAction("Orbit");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 panVector = pan.ReadValue<Vector3>();

        if (panVector != Vector3.zero)
        {
            PanCamera(panVector);
        }

        if (orbit.IsPressed())
        {
            OrbitCamera();
        }
    }

    void PanCamera(Vector3 moveDir)
    {
        transform.position += moveDir * panSpeed * Time.deltaTime;
    }

    void OrbitCamera()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        //float mouseX = Input.GetAxis("Mouse X");
        //float mouseY = Input.GetAxis("Mouse Y");

        transform.RotateAround(transform.position, Vector3.up, mouseDelta.x);
        transform.RotateAround(transform.position, transform.right, -mouseDelta.y);
    }
}
