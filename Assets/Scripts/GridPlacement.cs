using UnityEngine;
using UnityEngine.InputSystem;

public class GridPlacement : MonoBehaviour
{
    [SerializeField]
    GameObject objectToPlace;
    GameObject previewObject;
    Renderer previewRenderer;

    [SerializeField]
    Grid grid;

    [SerializeField]
    Camera buildCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previewObject = Instantiate(objectToPlace);
        previewObject.layer = 2;
        previewRenderer = previewObject.GetComponent<Renderer>();
        Color previewColor = previewRenderer.material.color;
        previewRenderer.material.color = new Color(previewColor.r, previewColor.g, previewColor.b, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = MouseToWorldSpace();
        Vector3Int cellPos = grid.WorldToCell(mousePos);
        previewObject.transform.position = grid.GetCellCenterWorld(cellPos);
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Instantiate(objectToPlace, previewObject.transform.position, Quaternion.identity);
        }
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
