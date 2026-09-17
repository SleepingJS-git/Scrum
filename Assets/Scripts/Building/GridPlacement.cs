using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridPlacement : MonoBehaviour
{
    // The prefab being placed and its script
    [SerializeField]
    Buildable startingBuildable;
    Buildable buildable;
    GameObject objectToPlace;

    // Track the preview for the placeable object
    GameObject previewObject;
    Renderer previewRenderer;

    // The grid being placed on
    [SerializeField]
    Grid grid;

    // The camera being used for building
    [SerializeField]
    Camera buildCam;

    // The current rotation of the object
    float rotation = 0;

    // All input actions
    private PlayerInput playerInput;
    private InputAction placeAction;
    private InputAction rotateLeftAction;
    private InputAction rotateRightAction;

    // Dictionary representing which cells are occupied and what they are occupied with
    private Dictionary<Vector3Int, int> occupiedCells = new Dictionary<Vector3Int, int>();

    // The offset of the object being placed
    private Vector3 offset = Vector3.zero;

    // The default color of the placed object
    private Color defaultColor;

    // The maximum number of building placements/placement tracking
    public int maxPlacements;
    public int currentPlacements;

    // UI used to determine whether we're hovering the buildable selection bar
    [SerializeField] private BuildingUI buildingUI;
    [SerializeField] private BarHoverCheck barCheck;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize all inputs
        playerInput = GetComponent<PlayerInput>();
        placeAction = playerInput.actions.FindAction("Place");
        rotateLeftAction = playerInput.actions.FindAction("Rotate Left");
        rotateRightAction = playerInput.actions.FindAction("Rotate Right");

        buildable = null;
        // Update the info of the buildable object
        UpdateBuildable();
    }

    // Update is called once per frame
    void Update()
    {
        // Check that conditions allow for previewing the buildable
        if (!CanPreviewBuildable())
        {
            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }

            return;
        }

        previewObject.SetActive(true);

        // If the right rotate action is pressed, increase the rotation angle
        if (rotateRightAction.WasPressedThisFrame())
        {
            rotation += 90;

            // If the rotation angle goes above 360
            if (rotation >= 360)
            {
                rotation = 0;
            }

        }
        // If the left rotate action is pressed, decrease the rotation angle
        if (rotateLeftAction.WasPressedThisFrame())
        {
            // If the rotation angle is 0 or less go up to 360
            if (rotation <= 0)
            {
                rotation = 360;
            }
            rotation -= 90;
        }


        // Set the rotation to the current rotation value
        previewObject.transform.rotation = Quaternion.Euler(0, rotation, 0);

        // Find the position of the mouse on the grid, show the preview there
        Vector3 mousePos = MouseToWorldSpace();
        Vector3Int cellPos = grid.WorldToCell(mousePos);

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

        // If any of the cells of the object are taken, change color to red
        if (IsAnyCellTaken(cellPos, buildable))
        {
            previewRenderer.material.color = new Color(2f, defaultColor.g, defaultColor.b, 0.1f);
        }
        else
        {
            previewRenderer.material.color = defaultColor;
        }

        // Set the preview object's position to the grid cell
        previewObject.transform.position = grid.CellToWorld(cellPos);

        // Offset the object's position based on its rotation
        previewObject.transform.position += (Quaternion.Euler(0, rotation, 0) * (offset - new Vector3(0.5f, 0.5f, 0.5f))) + new Vector3(0.5f, 0.5f, 0.5f);

        // If left-clicked, create the object at the location of the preview
        if (placeAction.WasPressedThisFrame())
        {
            if (!IsAnyCellTaken(cellPos, buildable))
            {
                Instantiate(objectToPlace, previewObject.transform.position, Quaternion.Euler(0, rotation, 0));
                SetCellsToOccupied(buildable, cellPos, 0);
                currentPlacements++;
            }
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
    /// Check if any of the cells that the placeable object occupies are taken
    /// </summary>
    /// <param name="originPosition"> The origin position of the object being placed</param>
    /// <param name="buildable"> The buildable script of the object being placed </param>
    /// <returns></returns>
    bool IsAnyCellTaken(Vector3Int originPosition, Buildable buildable)
    {
        for (int i = 0; i < buildable.OccupiedCells.Length; i++)
        {
            if (IsCellTaken(originPosition + Vector3Int.RoundToInt(Quaternion.Euler(0, rotation, 0) * buildable.OccupiedCells[i])))
            {
                return true;
            }
        }
        return false;
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

    /// <summary>
    /// Sets all cells of the object being placed to occupid
    /// </summary>
    /// <param name="buildable"> Buildable script attached to placeable object</param>
    /// <param name="originPos"> The origin position of the object being placed </param>
    /// <param name="objectID"> The object ID for the object being placed </param>
    void SetCellsToOccupied(Buildable buildable, Vector3Int originPos, int objectID = 0)
    {
        for (int i = 0; i < buildable.OccupiedCells.Length; i++)
        {
            OccupyCell(originPos + Vector3Int.RoundToInt(Quaternion.Euler(0, rotation, 0) * buildable.OccupiedCells[i]), objectID);
        }
    }

    /// <summary>
    /// Update the offset variable to the offset of the current object
    /// </summary>
    void UpdateOffset()
    {
        offset = buildable.PivotOffset;
    }

    /// <summary>
    /// Update all values of the current buildable to be accurate to the current object
    /// </summary>
    private void UpdateBuildable()
    {
        objectToPlace = buildable.gameObject;
        // Create the preview object and make it transparent
        previewObject = Instantiate(objectToPlace);
        previewRenderer = previewObject.GetComponent<Buildable>().ObjectRenderer;
        previewObject.layer = 2;
        previewRenderer.gameObject.layer = 2;
        UpdateOffset();
        Debug.Log(offset);
        defaultColor = previewRenderer.material.color;
        previewRenderer.material.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.1f);
        defaultColor = previewRenderer.material.color;
    }

    public void SetBuildable(Buildable newBuildable)
    {
        buildable = newBuildable;
        UpdateBuildable();
    }

    private bool CanPreviewBuildable()
    {
        // Nothing has been selected yet
        if (buildable == null)
            return false;

        // Player has used all their placements
        if (currentPlacements >= maxPlacements)
            return false;

        // Don't place/preview while interacting with the build UI
        if (barCheck.IsHoveringBuildableBar)
            return false;

        return true;
    }
}
