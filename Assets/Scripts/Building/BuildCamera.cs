using UnityEngine;
using UnityEngine.InputSystem;

public class BuildCamera : MonoBehaviour
{
    // Grid building script
    [SerializeField]
    private GridPlacement gridBuilding;

    // Player input and actions
    private PlayerInput playerInput;
    private InputAction pan;
    private InputAction orbit;
    private InputAction zoom;
    private InputAction drag;

    // Speed for the camera pan
    private float panSpeed = 20f;
    private float dragSensitivity = 20f;

    // Point for the camera to orbit around
    private Vector3 orbitTargetPoint;
    private float orbitSensitivity = 0.5f;

    // Maximum distance for raycast
    private float maxRayDist = 100f;

    // Zoom distance
    private float zoomDistance = 10f;
    private float minZoom = 2f;
    private float maxZoom = 20f;
    private float zoomSensitivity = 0.5f;

    // Pitch constraints
    private float minPitch = 10f;
    private float maxPitch = 90f;

    private float currentX = 0f;
    private float currentY = 0f;

    private Vector3 targetCamPosition;
    private Quaternion targetRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Set up inputs 
        playerInput = gridBuilding.GetComponent<PlayerInput>();
        pan = playerInput.actions.FindAction("Pan");
        orbit = playerInput.actions.FindAction("Orbit");
        zoom = playerInput.actions.FindAction("Zoom");
        drag = playerInput.actions.FindAction("Drag");
        targetCamPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Find the direction to pan
        Vector3 panVector = pan.ReadValue<Vector3>().normalized;

        // Find the value of the scroll
        float scrollValue = zoom.ReadValue<float>() * zoomSensitivity;

        // If panning, call pan function
        if (panVector != Vector3.zero)
        {
            PanCamera(panVector);
        }
        if (drag.IsPressed() && ! orbit.IsPressed())
        {
            DragPan();
        }

        // If scrolling zoom
        if (scrollValue != 0)
        {
            zoomDistance = Mathf.Clamp(zoomDistance - scrollValue, minZoom, maxZoom);
        }

        // Get the target point to orbit around
        orbitTargetPoint = GetOrbitTarget();


        // If orbiting, call orbit function
        if (orbit.IsPressed())
        {
            OrbitCamera();
        }

        transform.position = Vector3.Lerp(transform.position, targetCamPosition, 2f * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);

        // Set the distance of the camera based on the zoom
        //float currentDistance = Mathf.Lerp(-transform.localPosition.z, zoomDistance, Time.deltaTime * 2f);
        //transform.position -= new Vector3(0, 0, currentDistance);
    }


    /// <summary>
    /// Pan the camera in the direction of the input
    /// </summary>
    /// <param name="moveDir">Direction to move</param>
    void PanCamera(Vector3 moveDir)
    {
        Vector3 rotatedMoveDir = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0) * moveDir;
        targetCamPosition += rotatedMoveDir * panSpeed * Time.deltaTime;
    }

    void DragPan()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        Vector3 mouseDeltaTo3D = new Vector3(mouseDelta.x, 0, mouseDelta.y);
        targetCamPosition -= Quaternion.Euler(0, targetRotation.eulerAngles.y, 0) * (mouseDeltaTo3D * Time.deltaTime * dragSensitivity);
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

        // Find the mouse movement
        //Vector2 mouseDelta = Mouse.current.delta.ReadValue() * orbitSensitivity;
        //float newXRotation = transform.eulerAngles.x - mouseDelta.y;

        //// If the movement would rotate the camera outside of its constraints do not do the movement
        //if (newXRotation < minPitch || newXRotation > maxPitch)
        //{
        //    mouseDelta = new Vector2(mouseDelta.x, 0);
        //}

        
        //// Rotate around the orbit target
        //transform.RotateAround(orbitTargetPoint, Vector3.up, mouseDelta.x);
        //transform.RotateAround(orbitTargetPoint, transform.right, -mouseDelta.y);

        //targetCamPosition = transform.position;
    }

    /// <summary>
    /// Get the point to orbit around
    /// </summary>
    /// <returns>Vector 3 orbit point</returns>
    Vector3 GetOrbitTarget()
    {
        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out hit, maxRayDist))
        {
            return hit.point;
        }
        return transform.position + (transform.forward * zoomDistance);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targetCamPosition, 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(orbitTargetPoint, 0.5f);
    }
}
