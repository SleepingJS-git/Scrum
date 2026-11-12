using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    [Header("UI Slots")]
    [SerializeField] private BuildSelectionSlot[] slots;
    [SerializeField] private BarHoverCheck barHoverCheck;
    [SerializeField] private GridPlacement gridPlacement;
    [SerializeField] private TMP_Text placementInfo;
    [SerializeField] private TMP_Text selectedWeightInfo;

    public GameObject SelectedPrefab { get; private set; }

    public event Action<GameObject> OnSelectedPrefabChanged;

    private bool choicesGenerated;
    private bool wasBuilding;

    public Image weightBar;
    public Color maxWeightColor, medWeightColor, lowWeightColor;

    private void OnEnable()
    {
        ClearSelection();
        wasBuilding = GridPlacement.IsBuildingPhase;
        if (selectedWeightInfo)
            selectedWeightInfo.text = string.Empty;
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
        if (!gridPlacement) return;
        bool isBuilding = GridPlacement.IsBuildingPhase;

        if (wasBuilding && !isBuilding) ClearSelection();

        wasBuilding = isBuilding;

        if (placementInfo) placementInfo.text = gridPlacement ? $"Placed Weight: {gridPlacement.currentWeight.Value}/{gridPlacement.currentMaxWeight}" : "";

        UpdateWeight(gridPlacement.currentWeight.Value, gridPlacement.currentMaxWeight);

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
        bool normalChosen = false;
        bool trapChosen = false;
        // Clears both the UI selection and the actual placement selection.
        ClearSelection();

        List<ulong> available = new List<ulong>(BuildableDatabase.AvailablePrefabIds);
        List<ulong> availableNormals = new List<ulong>();
        List<ulong> availableTraps = new List<ulong>();

        for (int i = 0; i < available.Count; i++)
        {
            Buildable.BuildType currentType = BuildableDatabase.GetBuildable(available[i]).Type;
            if (currentType == Buildable.BuildType.Basic)
            {
                availableNormals.Add(available[i]);
            }
            else if (currentType == Buildable.BuildType.Trap)
            {
                availableTraps.Add(available[i]);
            }
        }

        if (availableNormals.Count <= 0) { normalChosen = true; }
        if (availableTraps.Count <= 0) { trapChosen = true; }

        foreach (BuildSelectionSlot slot in slots)
        {
            if (!slot) continue;

            if (available.Count == 0)
            {
                slot.Clear();
                continue;
            }
            int randomIndex;
            ulong chosenPrefabId;

            if (!normalChosen)
            {
                randomIndex = UnityEngine.Random.Range(0, availableNormals.Count);
                chosenPrefabId = availableNormals[randomIndex];
                availableNormals.RemoveAt(randomIndex);
                available.Remove(chosenPrefabId);
                normalChosen = true;
            }
            else if (!trapChosen)
            {
                randomIndex = UnityEngine.Random.Range(0, availableTraps.Count);
                chosenPrefabId = availableTraps[randomIndex]; 
                availableTraps.RemoveAt(randomIndex);
                available.Remove(chosenPrefabId);
                trapChosen = true;
            }
            else
            {
                randomIndex = UnityEngine.Random.Range(0, available.Count);
                chosenPrefabId = available[randomIndex];
                available.RemoveAt(randomIndex);
            }

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

        if (selectedWeightInfo)
            selectedWeightInfo.text = $"Object Weight: {prefab.weight}";

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

    public void UpdateWeight(float currentWeight, float maxWeight)
    {

        float perc = (float)currentWeight / maxWeight;

        weightBar.fillAmount = perc;

        if (currentWeight >= maxWeight) weightBar.color = maxWeightColor;
        else if (currentWeight >= maxWeight / 2) weightBar.color = medWeightColor;
        else weightBar.color = lowWeightColor;
    }
}