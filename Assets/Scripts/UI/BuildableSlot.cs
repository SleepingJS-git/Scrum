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

    public ulong BuildPrefabID { get; private set; }

    private Action<BuildSelectionSlot> onClicked;

    public void Setup(ulong prefabID, Action<BuildSelectionSlot> clickAction)
    {
        BuildPrefabID = prefabID;
        onClicked = clickAction;

        nameText.text = BuildableDatabase.GetBuildable(prefabID).name;

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