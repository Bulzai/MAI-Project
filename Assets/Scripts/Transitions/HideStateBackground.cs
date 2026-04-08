using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideStateBackground : MonoBehaviour
{
    public GameObject nextUIElements;
    public GameObject nextStateGo;
    public GameObject gameWorld;
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
            case 4:
                GameEvents.OnMainGameStateEntered -= ContinueNextState;
                break;
            case 5:
                GameEvents.OnMainGameStateExited -= ContinueNextState;
                break;
            default:
                break;
        }
    }

    private void ContinueNextState()
    {
        if (nextState == 3) gameWorld.SetActive(true);
        if (nextState != 2 && nextState != 4 && nextState != 5) nextUIElements.SetActive(true);
        if (nextState != 4) nextStateGo.SetActive(true);

        if (nextState != 5) currentUIElements.SetActive(false);

        if (nextState != 0 && nextState != 3) currentState.SetActive(false);
    }
}