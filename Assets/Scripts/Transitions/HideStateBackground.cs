using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideStateBackground : MonoBehaviour
{
    public GameObject nextUIElements;
    public GameObject nextStateGo;
    public int nextState;


    public GameObject currentUIElements;
    public GameObject currentState;

    private void OnEnable()
    {
        switch (nextState)
        {
            case 0:
                GameEvents.OnPlayerSelectionStateEntered += ContinueNextState;
                break;
            case 1:
                GameEvents.OnPlaceableItemSelectionStateEntered += ContinueNextState;
                break;
            case 2:
                GameEvents.OnSurpriseBoxStateEntered += ContinueNextState;
                break;
            case 3:
                GameEvents.OnPlaceItemStateEntered += ContinueNextState;
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        switch (nextState)
        {
            case 0:
                GameEvents.OnPlayerSelectionStateEntered -= ContinueNextState;
                break;
            case 1:
                GameEvents.OnPlaceableItemSelectionStateEntered -= ContinueNextState;
                break;
            case 2:
                GameEvents.OnSurpriseBoxStateEntered -= ContinueNextState;
                break;
            case 3:
                GameEvents.OnPlaceItemStateEntered -= ContinueNextState;
                break;
            default:
                break;
        }
    }

    private void ContinueNextState()
    {
        if (nextState != 2) nextUIElements.SetActive(true);
        nextStateGo.SetActive(true);

        currentUIElements.SetActive(false);

        if (nextState != 0 && nextState != 3) currentState.SetActive(false);
    }
}