using UnityEngine;
using UnityEngine.UIElements;

public class GridVisual : MonoBehaviour
{
    [SerializeField]
    Grid grid;

    public int length = 10;

    public int width = 10;

    public int height = 10;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        for (int y = 0; y <= height; y++)
        {
            for (int z = 0; z <= length; z++)
            {
                Vector3 start = transform.position + new Vector3(0, y, z * grid.cellSize.z);
                Vector3 end = transform.position + new Vector3(width * grid.cellSize.x, y, z * grid.cellSize.z);
                Gizmos.DrawLine(start, end);
            }
            for (int x = 0; x <= width; x++)
            {
                Vector3 start = transform.position + new Vector3(x * grid.cellSize.x, y, 0);
                Vector3 end = transform.position + new Vector3(x * grid.cellSize.x, y, length * grid.cellSize.z);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
