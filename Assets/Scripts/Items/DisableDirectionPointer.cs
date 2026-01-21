using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableDirectionPointer : MonoBehaviour
{
    public GameObject directionPointer;

    private void OnEnable()
    {
        PlaceItemState.CountDownStarted += DeactiveDirectionPointer;
    }
    private void OnDisable()
    {
        PlaceItemState.CountDownStarted -= DeactiveDirectionPointer;

    }
    void DeactiveDirectionPointer()
    {
        if (directionPointer != null)
            directionPointer.SetActive(false);
    }
}
