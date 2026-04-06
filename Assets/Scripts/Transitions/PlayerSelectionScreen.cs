using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelectionScreen : MonoBehaviour
{
    public GameObject Background;

    private void OnEnable()
    {
        GameEvents.OnPlaceableItemSelectionStateEntered += HideBackground;
    }

    private void OnDisable()
    {
        GameEvents.OnPlaceableItemSelectionStateEntered -= HideBackground;
    }

    private void HideBackground()
    {
        if (Background != null)
            Background.SetActive(false);
    }
}