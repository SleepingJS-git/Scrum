using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingUI : MonoBehaviour
{
    [Header("UI Slots")]
    [SerializeField] private BuildSelectionSlot[] slots;
    [SerializeField] private BarHoverCheck barHoverCheck;
    [SerializeField] private GridPlacement gridPlacement;
    [SerializeField] private TMP_Text placementInfo;

    public GameObject SelectedPrefab { get; private set; }

    public event Action<GameObject> OnSelectedPrefabChanged;

    private bool choicesGenerated;
    private bool wasBuilding;

    private void OnEnable()
    {
        ClearSelection();
        wasBuilding = GridPlacement.IsBuildingPhase;
    }

    private void Start()
    {
        // PlayerManager might generate choices before Start runs.
        if (!choicesGenerated) GenerateRandomChoices();
    }

    private void OnDisable()
    {
        ClearSelection();
    }

    public void SetGridPlacement(GridPlacement gp)
    {
        if (gridPlacement && gridPlacement != gp) gridPlacement.ClearBuildable();

        gridPlacement = gp;

        if (gridPlacement) gridPlacement.barCheck = barHoverCheck;

        ClearSelection();
    }

    private void Update()
    {
        bool isBuilding = GridPlacement.IsBuildingPhase;

        if (wasBuilding && !isBuilding) ClearSelection();

        wasBuilding = isBuilding;

        if (placementInfo) placementInfo.text = gridPlacement ? $"{gridPlacement.currentPlacements.Value}/{gridPlacement.maxPlacements}" : "";

        if (!isBuilding || !gridPlacement || !gridPlacement.IsSpawned || !gridPlacement.IsOwner) return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame) SelectIndex(0);
        else if (keyboard.digit2Key.wasPressedThisFrame) SelectIndex(1);
        else if (keyboard.digit3Key.wasPressedThisFrame) SelectIndex(2);
        else if (keyboard.digit4Key.wasPressedThisFrame) SelectIndex(3);
    }

    public void GenerateRandomChoices()
    {
        // Clears both the UI selection and the actual placement selection.
        ClearSelection();

        List<ulong> available = new List<ulong>(BuildableDatabase.AvailablePrefabIds);

        foreach (BuildSelectionSlot slot in slots)
        {
            if (!slot) continue;

            if (available.Count == 0)
            {
                slot.Clear();
                continue;
            }

            int randomIndex = UnityEngine.Random.Range(0, available.Count);
            ulong chosenPrefabId = available[randomIndex];

            available.RemoveAt(randomIndex);

            slot.Setup(chosenPrefabId, SelectSlot);
        }

        choicesGenerated = true;
    }

    public void ClearSelection()
    {
        if (slots != null)
        {
            foreach (BuildSelectionSlot slot in slots)
            {
                if (slot) slot.SetHighlighted(false);
            }
        }

        SelectedPrefab = null;

        if (gridPlacement) gridPlacement.ClearBuildable();

        OnSelectedPrefabChanged?.Invoke(null);
    }

    private void SelectIndex(int index)
    {
        if (index < slots.Length && slots[index]) slots[index].Select();
    }

    private void SelectSlot(BuildSelectionSlot selectedSlot)
    {
        if (!isActiveAndEnabled || !GridPlacement.IsBuildingPhase || !selectedSlot.HasChoice) return;
        if (!gridPlacement || !gridPlacement.IsSpawned || !gridPlacement.IsOwner) return;

        foreach (BuildSelectionSlot slot in slots)
        {
            if (slot) slot.SetHighlighted(slot == selectedSlot);
        }

        Buildable prefab = BuildableDatabase.GetBuildable(selectedSlot.BuildPrefabID);

        SelectedPrefab = prefab ? prefab.gameObject : null;

        gridPlacement.SetBuildable(selectedSlot.BuildPrefabID);

        OnSelectedPrefabChanged?.Invoke(SelectedPrefab);
    }

    public bool IsAnySlotHovered()
    {
        foreach (BuildSelectionSlot slot in slots)
        {
            if (slot && slot.IsHovered) return true;
        }

        return false;
    }
}