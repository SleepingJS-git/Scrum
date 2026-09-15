using UnityEngine;
using UnityEngine.InputSystem;

public class BuildCamera : MonoBehaviour
{
    [SerializeField]
    private GridPlacement gridBuilding;
    private PlayerInput playerInput;
    private InputAction pan;

    private float panSpeed = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = gridBuilding.GetComponent<PlayerInput>();
        pan = playerInput.actions.FindAction("Pan");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 panVector = pan.ReadValue<Vector3>();

        if (panVector != Vector3.zero)
        {
            PanCamera(panVector);
        }
    }

    void PanCamera(Vector3 moveDir)
    {
        transform.position += moveDir * panSpeed * Time.deltaTime;
    }
}
