using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject ItemSelectionPanel;
    public GridPlacementSystem placementSystem;
    [SerializeField] private GameObject grid;

    public void ToggleItemSelectionPanel()
    {
        ItemSelectionPanel.SetActive(!ItemSelectionPanel.activeSelf);
    }

    public void Toggle()
    {
        grid.SetActive(!grid.activeSelf);
    }

    private void OnEnable()
    {
        GameEvents.OnItemSelectionPanelOpened += ToggleItemSelectionPanel;
        GameEvents.OnToggleGrid += Toggle;
    }

    private void OnDisable()
    {
        GameEvents.OnItemSelectionPanelOpened -= ToggleItemSelectionPanel;
        GameEvents.OnToggleGrid -= Toggle;
    }
}
