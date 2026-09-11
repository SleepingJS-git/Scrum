using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class Buildable : MonoBehaviour
{
    [SerializeField]
    private Vector3Int[] occupiedCells;

    [SerializeField]
    private GameObject mesh;

    private Vector3Int dimensions;

    public Vector3Int[] OccupiedCells
    {
        get { return occupiedCells; }
    }
    public Vector3Int Dimensions
    {
        get { return dimensions; }
    }
    public Vector3 PivotOffset
    {
        get
        {
            SetDimensions();
            Debug.Log(dimensions);
            return new Vector3(dimensions[0] / 2f, 0.5f, dimensions[2] / 2f);
        }
    }
    public int TotalGridSpaces
    {
        get { return occupiedCells.Length; }
    }
    public Renderer ObjectRenderer
    {
        get { return mesh.GetComponent<Renderer>(); }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetDimensions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetDimensions()
    {
        int lowestX = 0;
        int highestX = 0;
        int lowestZ = 0;
        int highestZ = 0;
        int lowestY = 0;
        int highestY = 0;
        for (int i = 0; i < occupiedCells.Length; i++)
        {
            if (occupiedCells[i][0] > highestX)
            {
                highestX = occupiedCells[i][0];
            }
            else if (occupiedCells[i][0] < lowestX)
            {
                lowestX = occupiedCells[i][0];
            }

            if (occupiedCells[i][2] > highestZ)
            {
                highestZ = occupiedCells[i][2];
            }
            else if (occupiedCells[i][2] < lowestZ)
            {
                lowestZ = occupiedCells[i][2];
            }

            if (occupiedCells[i][1] > highestY)
            {
                highestY = occupiedCells[i][1];
            }
            else if (occupiedCells[i][1] < lowestY)
            {
                lowestY = occupiedCells[i][1];
            }
        }
        dimensions = new Vector3Int((highestX - lowestX) + 1, (highestY - lowestY) + 1, (highestZ - lowestZ) + 1);
    }
}
