using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridPlacement : NetworkBehaviour
{
    // The prefab being placed and its script
    [SerializeField]
    Buildable startingBuildable;
    Buildable buildable;

    // Track the preview for the placeable object
    Buildable previewObject;
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
    private Dictionary<Vector3Int, ulong> occupiedCells = new Dictionary<Vector3Int, ulong>();

    // The offset of the object being placed
    private Vector3 offset;

    // The default color of the placed object
    private Color defaultColor;

    // The maximum number of building placements/placement tracking
    public int maxPlacements;
    public NetworkVariable<int> currentPlacements = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // UI used to determine whether we're hovering the buildable selection bar
    [SerializeField] private BuildingUI buildingUI;
    [HideInInspector] public BarHoverCheck barCheck;
    private bool _canPreviewBuildable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        buildCam.gameObject.SetActive(IsOwner);
        grid = GameObject.Find("Plane (Grid)").GetComponent<Grid>();

        if (!IsOwner) return;
        // Initialize all inputs
        GameObject buildingUIObj = GameObject.Find("BuildingUI");
        buildingUIObj.GetComponent<BuildingUI>().SetGridPlacement(this);

        playerInput = GetComponent<PlayerInput>();
        placeAction = playerInput.actions.FindAction("Place");
        rotateLeftAction = playerInput.actions.FindAction("Rotate Left");
        rotateRightAction = playerInput.actions.FindAction("Rotate Right");

        buildable = null;
        // Update the info of the buildable object
        // UpdateBuildable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        // Controls

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

        if (barCheck.IsHoveringBuildableBar) return;

        // Find the position of the mouse on the grid, show the preview there
        Vector3 mousePos = MouseToWorldSpace();
        Vector3Int cellPos = grid.WorldToCell(mousePos);


        // If left-clicked, create the object at the location of the preview
        if (placeAction.WasPressedThisFrame())
        {
            PlaceBuildableServerRpc(cellPos, rotation);
        }

        PreviewObjectServerRpc(mousePos, cellPos, rotation);
    }

    [Rpc(SendTo.Server)]
    private void PreviewObjectServerRpc(Vector3 mousePos, Vector3Int cellPos, float rotation)
    {
        // Check that conditions allow for previewing the buildable
        bool isObjectVisible = CanPreviewBuildable();
        bool isAnyCellTaken = false;

        if (buildable) isAnyCellTaken = IsAnyCellTaken(cellPos, buildable);
        
        if (isObjectVisible)
        {
            // Set the rotation to the current rotation value
            previewObject.transform.rotation = Quaternion.Euler(0, rotation, 0);

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

            // Set the preview object's position to the grid cell
            previewObject.transform.position = grid.CellToWorld(cellPos);

            // Offset the object's position based on its rotation
            previewObject.transform.position += (Quaternion.Euler(0, rotation, 0) * (offset - new Vector3(0.5f, 0.5f, 0.5f))) + new Vector3(0.5f, 0.5f, 0.5f);
        }

        // If any of the cells of the object are taken, change color to red
        SetColorClientRpc(isAnyCellTaken, isObjectVisible);
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void SetColorClientRpc(bool isAnyCellTaken, bool isObjectVisible)
    {
        if (!previewObject) return;

        if (isObjectVisible)
            previewObject.gameObject.SetActive(true);
        else
        {
            previewObject.gameObject.SetActive(false);
            return;
        }
        if (isAnyCellTaken)
            previewRenderer.material.color = new Color(2f, defaultColor.g, defaultColor.b, 0.1f);
        else
            previewRenderer.material.color = defaultColor;
    }

    [Rpc(SendTo.Server)]
    private void PlaceBuildableServerRpc(Vector3Int cellPos, float rotation)
    {
        if (!IsAnyCellTaken(cellPos, buildable))
        {
            // Send Server Request to create object
            Buildable obj = Instantiate(buildable, previewObject.transform.position, Quaternion.Euler(0, rotation, 0));
            obj.NetworkObject.Spawn();
            SetCellsToOccupied(buildable, cellPos);
            currentPlacements.Value++;
            return;
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
    /// <param name="buildableId"> ID of the buildable at that position (Currently in use! :) )</param>
    void OccupyCell(Vector3Int position, ulong buildableId)
    {
        if (!IsCellTaken(position))
        {
            occupiedCells.Add(position, buildableId);
        }
    }

    /// <summary>
    /// Sets all cells of the object being placed to occupid
    /// </summary>
    /// <param name="buildable"> Buildable script attached to placeable object</param>
    /// <param name="originPos"> The origin position of the object being placed </param>
    /// <param name="objectID"> The object ID for the object being placed </param>
    void SetCellsToOccupied(Buildable buildable, Vector3Int originPos)
    {
        for (int i = 0; i < buildable.OccupiedCells.Length; i++)
        {
            OccupyCell(originPos + Vector3Int.RoundToInt(Quaternion.Euler(0, rotation, 0) * buildable.OccupiedCells[i]), buildable.Id);
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
    [Rpc(SendTo.Server)]
    private void UpdateBuildableServerRpc(ulong newBuildableID)
    {
        buildable = BuildableDatabase.GetBuildable(newBuildableID);
        if (!buildable) return;

        Debug.Log("There is a buildable?: " + buildable.name);
        if (previewObject != null) Destroy(previewObject.gameObject);
        // Create the preview object and make it transparent
        previewObject = Instantiate(buildable);
        previewObject.NetworkObject.Spawn();
        UpdateOffset();
        Debug.Log(offset);
        PreviewClientRpc(previewObject.NetworkObjectId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PreviewClientRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            previewObject = networkObject.GetComponent<Buildable>();
            previewObject.gameObject.layer = 2;

            previewRenderer = previewObject.ObjectRenderer;
            previewRenderer.gameObject.layer = 2;

            defaultColor = previewRenderer.material.color;
            previewRenderer.material.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.1f);
            defaultColor = previewRenderer.material.color;
        }
    }

    // public void SetBuildable(Buildable newBuildable)
    // {
    //     buildable = newBuildable;
    //     UpdateBuildable();
    // }

    public void SetBuildable(ulong newBuildableID)
    {
        // buildable = newBuildableID;
        Debug.Log("SetBuildable was called (ID): " + newBuildableID);
        UpdateBuildableServerRpc(newBuildableID);
    }

    private bool CanPreviewBuildable()
    {
        if (previewObject == null)
            return false;
        // Nothing has been selected yet
        if (buildable == null)
            return false;

        // Player has used all their placements
        if (currentPlacements.Value >= maxPlacements)
            return false;

        // Why tho?
        // // Don't place/preview while interacting with the build UI
        // if (barCheck.IsHoveringBuildableBar)
        //     return false;

        return true;
    }
}

/*
    TODO: separate networking things

    1. Inputs must remain on client:
        - Build Cam
        - Grid Placement Inputs

    2. Anything instantiated is called on Server using NetworkObject.Spawn()
        - Hologram buildings -> So that they are displayed to everyone
        - Actual Buildings -> Duh

    buildable variable?
    buildable is instantiated on server.
    server's local gp script changes to the new buildable
    client local gp script does not get those changes. but has buildableId? should it be networkvariable too?

    when client hovers on grid, needs to request to server to enable preview gameobject and change transform.

    grid.CellToWorld(cellPos) => Vector3.
    This needs to be passed into ServerRpc to move the transform.

    UpdateBuildableServerRpc(Vector3 pos)

    CanPreviewBuildable() looks like a client-side thing

    buildable is not a client variable. Its server
    currentPlacements looks like it should be changed to NetworkVariable
    barcheck is 100% a client side thing.

    IsCell() and IsAnyCellTaken() looks like a server-side thing because occupiedCells dict should be only on the server
*/
