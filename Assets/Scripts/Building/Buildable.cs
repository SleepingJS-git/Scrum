using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class Buildable : MonoBehaviour
{
    [SerializeField]
    private Vector2Int[] occupiedCells;

    [SerializeField]
    private GameObject mesh;

    public Vector2Int[] OccupiedCells
    {
        get { return occupiedCells; }
    }
    public Vector2 Dimensions
    {
        get
        {
            float length = 1;
            float width = 1;
            for (int i = 0; i < occupiedCells.Length; i++)
            {
                if (occupiedCells[i][0] + 1 > length)
                {
                    length = occupiedCells[i][0];
                }
                if (occupiedCells[i][1] + 1 > width)
                {
                    width = occupiedCells[i][1];
                }
            }
            return new Vector2(length, width);
            // SHOULD work for if the dimensions are increasing only in the positive direction, need to account for total difference positive and negative
        }
    }
    //public Vector2 PivotOffset
    //{
    //    get
    //    {
    //        Vector2 offset = 
    //    }
    //}
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
