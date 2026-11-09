using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildSelectionSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image background;
    [SerializeField] private BuildableModelPreview modelPreview;

    [Tooltip("The Raw Image's Rect Transform. Scale this, not the clickable button.")]
    [SerializeField] private RectTransform modelDisplay;

    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.3f, 0.45f);
    [SerializeField, Min(1f)] private float enlargedScale = 1.15f;
    [SerializeField, Min(0.1f)] private float animationSpeed = 12f;

    public ulong BuildPrefabID { get; private set; }
    public bool HasChoice { get; private set; }

    private Action<BuildSelectionSlot> onClicked;

    private bool hovered;
    private bool selected;
    private bool initialized;
    private Vector3 normalScale;
    private BuildingUI buildingUI;
    public bool IsHovered => hovered && HasChoice && isActiveAndEnabled;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) return;

        initialized = true;
        buildingUI = GetComponentInParent<BuildingUI>();

        if (modelDisplay) normalScale = modelDisplay.localScale;

        if (button)
        {
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(Clicked);
        }

        if (nameText) nameText.raycastTarget = false;
    }

    public void Setup(ulong prefabID, Action<BuildSelectionSlot> clickAction)
    {
        Initialize();

        BuildPrefabID = prefabID;
        onClicked = clickAction;

        Buildable prefab = BuildableDatabase.GetBuildable(prefabID);

        HasChoice = prefab != null;
        hovered = false;

        if (button) button.interactable = HasChoice;
        if (nameText) nameText.text = prefab ? prefab.name : "";
        if (modelPreview) modelPreview.ShowPrefab(prefab ? prefab.gameObject : null);

        SetHighlighted(false);
        SnapVisuals();
    }

    public void Clear()
    {
        Initialize();

        HasChoice = false;
        onClicked = null;
        hovered = false;

        if (button) button.interactable = false;
        if (nameText) nameText.text = "";
        if (modelPreview) modelPreview.ShowPrefab(null);

        SetHighlighted(false);
        SnapVisuals();
    }

    public void Select()
    {
        Clicked();
    }

    private void Clicked()
    {
        if (!isActiveAndEnabled || !HasChoice || (button && !button.interactable)) return;

        onClicked?.Invoke(this);
    }

    public void SetHighlighted(bool highlighted)
    {
        selected = highlighted && HasChoice;

        if (background) background.color = selected ? selectedColor : normalColor;
        if (modelPreview) modelPreview.SetSpinning(!selected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }

    private void Update()
    {
        bool enlargeModel = HasChoice && (hovered || selected);
        bool anySlotHovered = buildingUI != null && buildingUI.IsAnySlotHovered();
        bool showName = HasChoice && (hovered || (selected && !anySlotHovered));

        float t = 1f - Mathf.Exp(-animationSpeed * Time.unscaledDeltaTime);

        if (modelDisplay) modelDisplay.localScale = Vector3.Lerp(modelDisplay.localScale, normalScale * (enlargeModel ? enlargedScale : 1f), t);
        if (nameText) nameText.alpha = Mathf.Lerp(nameText.alpha, showName ? 1f : 0f, t);
    }

    private void OnDisable()
    {
        hovered = false;

        SetHighlighted(false);
        SnapVisuals();
    }

    private void SnapVisuals()
    {
        if (modelDisplay && initialized) modelDisplay.localScale = normalScale;
        if (nameText) nameText.alpha = 0f;
    }
}