using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class Buildable : MonoBehaviour
{
    [SerializeField]
    private Vector3Int[] occupiedCells;

    [SerializeField]
    private GameObject mesh;

    public Vector3Int[] OccupiedCells
    {
        get { return occupiedCells; }
    }
    public Vector3 Dimensions
    {
        get
        {
            float lowestX = 0;
            float highestX = 0;
            float lowestZ = 0;
            float highestZ = 0;
            float lowestY = 0;
            float highestY = 0;
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
            return new Vector3((highestX - lowestX) + 1, (highestY - lowestY) + 1, (highestZ - lowestZ) + 1);
        }
    }
    public Vector3 PivotOffset
    {
        get
        {
            Vector3 offset = new Vector3(Dimensions[0] / 2, 0, Dimensions[2] / 2);
            return offset;
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
