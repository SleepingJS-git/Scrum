using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildSelectionSlot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image background;

    [SerializeField] private Color normalColor = Color.gray;
    [SerializeField] private Color selectedColor = Color.white;

    public GameObject BuildPrefab { get; private set; }

    private Action<BuildSelectionSlot> onClicked;

    public void Setup(GameObject prefab, Action<BuildSelectionSlot> clickAction)
    {
        BuildPrefab = prefab;
        onClicked = clickAction;

        nameText.text = prefab.name;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Clicked);

        SetHighlighted(false);
    }

    private void Clicked()
    {
        onClicked?.Invoke(this);
    }

    public void SetHighlighted(bool highlighted)
    {
        background.color = highlighted ? selectedColor : normalColor;
    }
}