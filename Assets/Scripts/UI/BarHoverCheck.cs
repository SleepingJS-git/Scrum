using UnityEngine;
using UnityEngine.EventSystems;

public class BarHoverCheck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsHoveringBuildableBar { get; private set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHoveringBuildableBar = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHoveringBuildableBar = false;
    }

    // Rest of your BuildingUI code...
}