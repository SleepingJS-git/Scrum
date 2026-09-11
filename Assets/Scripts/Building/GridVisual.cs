using UnityEngine;
using UnityEngine.UIElements;

public class GridVisual : MonoBehaviour
{
    [SerializeField]
    Grid grid;

    public int length = 10;

    public int width = 10;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        for (int z = 0; z <= length; z++)
        {
            Vector3 start = transform.position + new Vector3(0, 0, z * grid.cellSize.z);
            Vector3 end = transform.position + new Vector3(width * grid.cellSize.x, 0, z * grid.cellSize.z);
            Gizmos.DrawLine(start, end);
        }
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = transform.position + new Vector3(x * grid.cellSize.x, 0, 0);
            Vector3 end = transform.position + new Vector3(x * grid.cellSize.x, 0, length * grid.cellSize.z);
            Gizmos.DrawLine(start, end);
        }
    }
}
