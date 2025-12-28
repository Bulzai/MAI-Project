using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;


public class UIInputModule : MonoBehaviour
{
    public static event Action OnCancelPressed;
    public static event Action OnSubmitPressed;

    
    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (GameEvents.CurrentState == GameState.PlayerSelectionState)
            OnCancelPressed?.Invoke();
        
        Debug.Log("UI Cancel pressed");
    }
    
    public void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (GameEvents.CurrentState == GameState.PlayerSelectionState)
            OnSubmitPressed?.Invoke();
        Debug.Log("UI OnSubmit pressed");

    }
}
