using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableDirectionPointer : MonoBehaviour
{
    public GameObject directionPointer;

    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += DeactiveDirectionPointer;
    }
    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= DeactiveDirectionPointer;

    }
    void DeactiveDirectionPointer()
    {
        if (directionPointer != null)
            directionPointer.SetActive(false);
    }
}
