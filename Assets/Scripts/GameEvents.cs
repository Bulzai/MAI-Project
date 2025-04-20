using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class GameEvents
{
    public static event Action OnItemSelectionPanelOpened;
    public static event Action OnToggleGrid;

    public static void ItemSelectionPanelOpened()
    {
        OnItemSelectionPanelOpened?.Invoke();
    }

    public static void ToggleGrid()
    {
        OnToggleGrid?.Invoke();
    }

}
