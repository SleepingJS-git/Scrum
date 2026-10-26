using UnityEngine;
using UnityEngine.InputSystem;

public class BarHoverCheck : MonoBehaviour
{
    [Tooltip("A fixed rectangle covering the whole bar, including gaps between slots. Defaults to this object's Rect Transform.")]
    [SerializeField] private RectTransform interactionArea;

    public bool IsHoveringBuildableBar
    {
        get
        {
            if (!isActiveAndEnabled || Mouse.current == null) return false;

            if (!interactionArea) interactionArea = transform as RectTransform;

            if (!interactionArea || !interactionArea.gameObject.activeInHierarchy) return false;

            Canvas canvas = interactionArea.GetComponentInParent<Canvas>();

            if (!canvas) return false;

            Canvas rootCanvas = canvas.rootCanvas;
            Camera eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

            return RectTransformUtility.RectangleContainsScreenPoint(interactionArea, Mouse.current.position.ReadValue(), eventCamera);
        }
    }
}