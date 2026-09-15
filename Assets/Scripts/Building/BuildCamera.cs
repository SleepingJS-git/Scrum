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

    // Speed for the camera pan
    private float panSpeed = 5;

    // Point for the camera to orbit around
    private Vector3 orbitTargetPoint;

    // Maximum distance for raycast
    private float maxRayDist = 100f;

    // Zoom distance
    private float zoomDistance = 10f;
    private float minZoom = 2f;
    private float maxZoom = 20f;

    // Pitch constraints
    private float minPitch = 10f;
    private float maxPitch = 90f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Set up inputs 
        playerInput = gridBuilding.GetComponent<PlayerInput>();
        pan = playerInput.actions.FindAction("Pan");
        orbit = playerInput.actions.FindAction("Orbit");
        zoom = playerInput.actions.FindAction("Zoom");
    }

    // Update is called once per frame
    void Update()
    {
        // Find the direction to pan
        Vector3 panVector = pan.ReadValue<Vector3>().normalized;

        // Find the value of the scroll
        float scrollValue = zoom.ReadValue<float>();

        // If panning, call pan function
        if (panVector != Vector3.zero)
        {
            PanCamera(panVector);
        }

        // If scrolling zoom
        if (scrollValue != 0)
        {
            zoomDistance = Mathf.Clamp(zoomDistance - scrollValue, minZoom, maxZoom);
        }

        // Get the target point to orbit around
        orbitTargetPoint = GetOrbitTarget();

        // Set the distance of the camera based on the zoom
        Vector3 dirToPoint = (orbitTargetPoint - transform.position).normalized;
        transform.position = orbitTargetPoint - (dirToPoint * zoomDistance);

        // If orbiting, call orbit function
        if (orbit.IsPressed())
        {
            OrbitCamera();
        }
    }


    /// <summary>
    /// Pan the camera in the direction of the input
    /// </summary>
    /// <param name="moveDir">Direction to move</param>
    void PanCamera(Vector3 moveDir)
    {
        Vector3 rotatedMoveDir = Quaternion.Euler(0, transform.eulerAngles.y, 0) * moveDir;
        transform.position += rotatedMoveDir * panSpeed * Time.deltaTime;
    }
    /// <summary>
    /// Orbit the camera around the orbit point
    /// </summary>
    void OrbitCamera()
    {
        // Find the mouse movement
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float newXRotation = transform.eulerAngles.x - mouseDelta.y;

        // If the movement would rotate the camera outside of its constraints do not do the movement
        if (newXRotation < minPitch || newXRotation > maxPitch)
        {
            mouseDelta = new Vector2(mouseDelta.x, 0);
        }

        // Rotate around the orbit target
        transform.RotateAround(orbitTargetPoint, Vector3.up, mouseDelta.x);
        transform.RotateAround(orbitTargetPoint, transform.right, -mouseDelta.y);
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

}
