using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridPlacement : MonoBehaviour
{
    // The prefab being placed
    [SerializeField]
    Buildable buildable;
    GameObject objectToPlace;

    // Track the preview for the placeable object
    GameObject previewObject;
    Renderer previewRenderer;

    // The grid being placed on
    [SerializeField]
    Grid grid;

    [SerializeField]
    Camera buildCam;

    float rotation = 0;

    private PlayerInput playerInput;
    private InputAction placeAction;
    private InputAction rotateLeftAction;
    private InputAction rotateRightAction;

    private Dictionary<Vector3Int, int> occupiedCells = new Dictionary<Vector3Int, int>();

    private Vector3 offset = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        placeAction = playerInput.actions.FindAction("Place");
        rotateLeftAction = playerInput.actions.FindAction("Rotate Left");
        rotateRightAction = playerInput.actions.FindAction("Rotate Right");

        objectToPlace = buildable.gameObject;
        // Create the preview object and make it transparent
        previewObject = Instantiate(objectToPlace);
        previewRenderer = previewObject.GetComponent<Buildable>().ObjectRenderer;
        previewObject.layer = 2;
        previewRenderer.gameObject.layer = 2;
        UpdateOffset();
        Debug.Log(offset);
        Color previewColor = previewRenderer.material.color;
        previewRenderer.material.color = new Color(previewColor.r, previewColor.g, previewColor.b, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        // Find the position of the mouse on the grid, show the preview there
        Vector3 mousePos = MouseToWorldSpace();
        Vector3Int cellPos = grid.WorldToCell(Quaternion.Euler(0, rotation, 0) * (mousePos - offset));

        // If the cell is occupied
        if (IsCellTaken(cellPos))
        {
            // Check if the mouse is on the x or y border of the grid space and shift the cell position accordingly
            if (mousePos.x % 1 == 0)
            {
                cellPos = new Vector3Int(cellPos.x - 1, cellPos.y, cellPos.z);
            }
            else
            {
                cellPos = new Vector3Int(cellPos.x, cellPos.y, cellPos.z - 1);
            }
        }

        // Set the preview object's position to the center of the grid cell
        previewObject.transform.position = grid.GetCellCenterWorld(cellPos);

        if (rotateRightAction.WasPressedThisFrame())
        {
            rotation += 90;
            if (rotation >= 360)
            {
                rotation = 0;
            }
        }
        if (rotateLeftAction.WasPressedThisFrame())
        {
            if (rotation <= 0)
            {
                rotation = 360;
            }
            rotation -= 90;
        }
        previewObject.transform.rotation = Quaternion.Euler(0, rotation, 0);
        // If left-clicked, create the object at the location of the preview
        if (placeAction.WasPressedThisFrame())
        {
            Instantiate(objectToPlace, previewObject.transform.position, Quaternion.Euler(0, rotation, 0));
            OccupyCell(cellPos, 0);
        }
    }

    /// <summary>
    /// Get the world space of the mouse based on a raycast from the camera
    /// </summary>
    /// <returns>World space of the mouse</returns>
    private Vector3 MouseToWorldSpace()
    {
        Ray ray = buildCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            Vector3 targetPos = hit.point;
            return targetPos;
        }

        return new Vector3(-100, -100, -100);
    }

    /// <summary>
    /// Check whether a cell on the grid is occupied
    /// </summary>
    /// <param name="position"> Position of the cell being checked </param>
    /// <returns> True if the cell is currently taken </returns>
    bool IsCellTaken(Vector3Int position)
    {
        return occupiedCells.ContainsKey(position);
    }

    /// <summary>
    /// Sets a cell to occupied
    /// </summary>
    /// <param name="position"> Position of the occupied cell </param>
    /// <param name="objectID"> ID of the object at that position (Currently useless, will update for tracking what type of object is placed </param>
    void OccupyCell(Vector3Int position, int objectID)
    {
        if (!IsCellTaken(position))
        {
            occupiedCells.Add(position, objectID);
        }
    }

    void UpdateOffset()
    {
        offset = buildable.PivotOffset;
    }
}
