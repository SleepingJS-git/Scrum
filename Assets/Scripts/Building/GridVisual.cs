using UnityEngine;
using UnityEngine.UIElements;

public class GridVisual : MonoBehaviour
{
    [SerializeField]
    bool on;

    [SerializeField]
    Grid grid;

    [SerializeField]
    Color gridColor;

    public int length = 10;

    public int width = 10;

    public int height = 10;

    private void OnDrawGizmos()
    {
        //if (!on) { return; }
        Gizmos.color = gridColor;

        for (int y = -(height/2); y <= height/2; y++)
        {
            for (int z = -(length/2); z <= length/2 ; z++)
            {
                Vector3 start = transform.position + new Vector3((-width/2), y, z * grid.cellSize.z);
                Vector3 end = transform.position + new Vector3((width * grid.cellSize.x)/2, y, z * grid.cellSize.z);
                Gizmos.DrawLine(start, end);
            }
            for (int x = -(length/2); x <= width/2; x++)
            {
                Vector3 start = transform.position + new Vector3(x * grid.cellSize.x, y, -(length/2));
                Vector3 end = transform.position + new Vector3(x * grid.cellSize.x, y, (length * grid.cellSize.z)/2);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
