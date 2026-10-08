using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildableDatabase : MonoBehaviour
{
    public static BuildableDatabase Instance;
    [SerializeField] private Buildable[] buildablePrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Grid grid;
    [SerializeField] private Vector3 minBuildBound;
    [SerializeField] private Vector3 maxBuildBound;
    [SerializeField] private LayerMask occupiedCellsLayerMask;
    [SerializeField] private GameObject testingCube;
    private Dictionary<ulong, Buildable> data;
    public static List<ulong> AvailablePrefabIds => Instance.data.Keys.ToList();
    public static Dictionary<Vector3Int, ulong> OccupiedCells
    {
        get; private set;
    }
    void Awake()
    {
        Instance = this;
        data = new Dictionary<ulong, Buildable>();
        foreach (Buildable b in buildablePrefabs)
            data[b.Id] = b;

        OccupiedCells = new Dictionary<Vector3Int, ulong>();
        if (spawnPoints.Length > 0 && grid != null)
        {
            SetSpawnPointsOccupied();
        }
        if (grid != null)
        {
            SetMapOccupiedSpaces();
            Debug.Log(OccupiedCells.Count);
        }
    }
    /// <summary>
    /// Check whether a cell on the grid is occupied
    /// </summary>
    /// <param name="position"> Position of the cell being checked </param>
    /// <returns> True if the cell is currently taken </returns>
    public static bool IsCellTaken(Vector3Int position)
    {
        return OccupiedCells.ContainsKey(position);
    }

    /// <summary>
    /// Check if any of the cells that the placeable object occupies are taken
    /// </summary>
    /// <param name="originPosition"> The origin position of the object being placed</param>
    /// <param name="buildable"> The buildable script of the object being placed </param>
    /// <returns></returns>
    public static bool IsAnyCellTaken(Vector3Int originPosition, Buildable buildable, float rotation)
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
    public static void OccupyCell(Vector3Int position, ulong buildableId)
    {
        if (!IsCellTaken(position))
        {
            OccupiedCells.Add(position, buildableId);
        }
    }

    /// <summary>
    /// Sets all cells of the object being placed to occupid
    /// </summary>
    /// <param name="buildable"> Buildable script attached to placeable object</param>
    /// <param name="originPos"> The origin position of the object being placed </param>
    /// <param name="objectID"> The object ID for the object being placed </param>
    public static void SetCellsToOccupied(Buildable buildable, Vector3Int originPos, float rotation)
    {
        Debug.Log("Occupied Cells:");
        foreach (Vector3Int pos in OccupiedCells.Keys)
        {
            Debug.Log(pos);
        }
        Debug.Log("New Incoming Cells:");
        for (int i = 0; i < buildable.OccupiedCells.Length; i++)
        {
            Debug.Log(originPos + Vector3Int.RoundToInt(Quaternion.Euler(0, rotation, 0) * buildable.OccupiedCells[i]));
            OccupyCell(originPos + Vector3Int.RoundToInt(Quaternion.Euler(0, rotation, 0) * buildable.OccupiedCells[i]), buildable.Id);
        }
    }
    public static Buildable GetBuildable(ulong id)
    {
        return Instance.data[id];
    }
    public static Buildable GetBuildableAtIndex(int idx)
    {
        return Instance.data.ElementAt(idx).Value;
    }
    private void SetSpawnPointsOccupied()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            BoxCollider bounds = spawnPoints[i].Find("Bounds").GetComponent<BoxCollider>();
            for (int x = 0; x < bounds.size.x; x++)
            {
                float currentOffsetX = spawnPoints[i].position.x + ((bounds.size.x / 2) - x);
                for (int y = 0; y < bounds.size.y; y++)
                {
                    float currentOffsetY = spawnPoints[i].position.y + ((bounds.size.y / 2) - y);
                    for (int z = 0; z < bounds.size.z; z++)
                    {
                        float currentOffsetZ = spawnPoints[i].position.z + ((bounds.size.z / 2) - z);
                        Vector3 offsetPos = new Vector3(currentOffsetX, currentOffsetY, currentOffsetZ);
                        Vector3Int cellPos = grid.WorldToCell(offsetPos);
                        OccupyCell(cellPos, 0);
                    }
                }
            }
        }
    }

    private void SetMapOccupiedSpaces()
    {
        Vector3Int minGridBound = grid.WorldToCell(minBuildBound);
        Vector3Int maxGridBound = grid.WorldToCell(maxBuildBound);
        Vector3Int spaceDifference = maxGridBound - minGridBound;

        for (int x = 0; x < spaceDifference.x; x++)
        {
            for(int y = 0; y < spaceDifference.y; y++)
            {
                for (int z = 0; z < spaceDifference.z; z++)
                {
                    Vector3Int currentSpace = minGridBound + new Vector3Int(x, y, z);
                    if (CheckSpace(currentSpace))
                    {
                        OccupyCell(currentSpace, 0);
                        Instantiate(testingCube, grid.GetCellCenterWorld(currentSpace), Quaternion.identity);
                    }
                }
            }
        }
    }

    private bool CheckSpace(Vector3Int currentSpace)
    {
        Collider[] hitColliders = Physics.OverlapBox(grid.GetCellCenterWorld(currentSpace), new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, occupiedCellsLayerMask);
        return (hitColliders.Length > 0);
    }
}
