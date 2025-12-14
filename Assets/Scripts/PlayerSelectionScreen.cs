using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelectionScreen : MonoBehaviour
{
    public GameObject Background;

    private void OnEnable()
    {
        GameEvents.OnSurpriseBoxStateEntered += HideBackground;
    }

    private void OnDisable()
    {
        GameEvents.OnSurpriseBoxStateEntered -= HideBackground;
    }

    private void HideBackground()
    {
        if (Background != null)
            Background.SetActive(false);
    }
}