using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildingUI : MonoBehaviour
{
    [Header("Available Buildables")]
    [SerializeField] private GameObject[] buildablePrefabs;

    [Header("UI Slots")]
    [SerializeField] private BuildSelectionSlot[] slots;

    public GameObject SelectedPrefab { get; private set; }

    public event Action<GameObject> OnSelectedPrefabChanged;

    [SerializeField] private GridPlacement gridPlacement;

    [SerializeField] private TMP_Text placementInfo;

    private void Start()
    {
        GenerateRandomChoices();
    }

    // always check for change in current placements for the text obj (public field in gridplacement)
    private void Update()
    {
        placementInfo.text = String.Format($"{gridPlacement.currentPlacements}/{gridPlacement.maxPlacements}");
    }

    private void GenerateRandomChoices()
    {
        List<GameObject> available = new List<GameObject>(buildablePrefabs);

        for (int i = 0; i < slots.Length; i++)
        {
            if (available.Count == 0)
                break;

            int randomIndex = UnityEngine.Random.Range(0, available.Count);

            //don't have duplicate buildables at certain slots
            GameObject chosenPrefab = available[randomIndex];
            available.RemoveAt(randomIndex);

            // adds the select slot method and prefab generated to each buildable button in the bar
            slots[i].Setup(chosenPrefab, SelectSlot);
        }
    }

    //
    private void SelectSlot(BuildSelectionSlot selectedSlot)
    {
        foreach (BuildSelectionSlot slot in slots)
        {
            slot.SetHighlighted(slot == selectedSlot);
        }

        Buildable selectedBuildable =
            selectedSlot.BuildPrefab.GetComponent<Buildable>();

        gridPlacement.SetBuildable(selectedBuildable);
    }
}