using UnityEngine;
using UnityEngine.InputSystem;

public class GridPlacement : MonoBehaviour
{
    [SerializeField]
    GameObject objectToPlace;

    [SerializeField]
    Grid grid;

    [SerializeField]
    Camera buildCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectToPlace = Instantiate(objectToPlace);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = MouseToWorldSpace();
        Vector3Int cellPos = grid.WorldToCell(mousePos);
        objectToPlace.transform.position = grid.GetCellCenterWorld(cellPos);
    }


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
}
