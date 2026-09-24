using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildCamera : NetworkBehaviour
{
    // Grid building script
    public GridPlacement gridBuilding;

    // Player input and actions
    private PlayerInput playerInput;
    private InputAction pan;
    private InputAction orbit;
    private InputAction zoom;
    private InputAction drag;

    // Speed for the camera pan
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float dragSensitivity = 0.25f;

    // Point for the camera to orbit around
    private Vector3 orbitTargetPoint;
    [SerializeField] private float orbitSensitivity = 0.5f;

    // Maximum distance for raycast
    private float maxRayDist = 100f;

    // Zoom distance
    private float zoomDistance = 10f;
    private float minZoom = 2f;
    private float maxZoom = 20f;
    [SerializeField] private float zoomSensitivity = 0.5f;

    // Pitch constraints
    private float minPitch = 10f;
    private float maxPitch = 90f;

    private float currentX = 0f;
    private float currentY = 0f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    [SerializeField]
    private Transform cameraTransform;
    float lerpDampening = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        playerInput = gridBuilding.GetComponent<PlayerInput>();

        playerInput.enabled = IsOwner;
        if (!IsOwner) return;

        //Set up inputs 
        pan = playerInput.actions.FindAction("Pan");
        orbit = playerInput.actions.FindAction("Orbit");
        zoom = playerInput.actions.FindAction("Zoom");
        drag = playerInput.actions.FindAction("Drag");

        currentX = transform.eulerAngles.y;
        currentY = transform.eulerAngles.x;

        targetPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        // Find the direction to pan
        Vector3 panVector = pan.ReadValue<Vector3>().normalized;

        // Find the value of the scroll
        float scrollValue = zoom.ReadValue<float>() * zoomSensitivity;

        // If panning, call pan function
        if (panVector != Vector3.zero)
        {
            PanCamera(panVector);
        }
        if (drag.IsPressed())
        {
            DragPan();
        }

        // If orbiting, call orbit function
        if (orbit.IsPressed())
        {
            OrbitCamera();
        }
        // If scrolling zoom
        if (scrollValue != 0)
        {
            zoomDistance = Mathf.Clamp(zoomDistance - scrollValue, minZoom, maxZoom);
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpDampening * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lerpDampening);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0f);

        // Set the distance of the camera based on the zoom
        float currentDistance = Mathf.Lerp(cameraTransform.localPosition.z,-zoomDistance, Time.deltaTime * lerpDampening);
        cameraTransform.localPosition = new Vector3(0, 0, currentDistance);
    }


    /// <summary>
    /// Pan the camera in the direction of the input
    /// </summary>
    /// <param name="moveDir">Direction to move</param>
    void PanCamera(Vector3 moveDir)
    {
        Vector3 rotatedMoveDir = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0) * moveDir;
        targetPosition += rotatedMoveDir * panSpeed * Time.deltaTime;
    }

    /// <summary>
    /// When the middle mouse button is dragged, drag the camera accordingly
    /// </summary>
    void DragPan()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        Vector3 mouseDeltaTo3D = new Vector3(mouseDelta.x, 0, mouseDelta.y);
        targetPosition -= Quaternion.Euler(0, targetRotation.eulerAngles.y, 0) * (mouseDeltaTo3D * Time.deltaTime * dragSensitivity * panSpeed);
    }

    /// <summary>
    /// Orbit the camera around the orbit point
    /// </summary>
    void OrbitCamera()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * orbitSensitivity;
        currentX += mouseDelta.x;
        currentY = Mathf.Clamp(currentY - mouseDelta.y, minPitch, maxPitch);

        targetRotation = Quaternion.Euler(currentY, currentX, 0);
    }

    public string DebugInfo()
    {
        return 
        $@"Build Cam Pos: {transform.position}
        Zoom Distance: {minZoom} - ({zoomDistance}) - {maxZoom}
        Current X: {currentX}
        Current Y: {currentY}
        ";
    }
}
