using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PausePlayerRoot : MonoBehaviour
{
    // Pause Menu Event
    public static event Action OnPauseEvent;
    
    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;              // only once per press
        Debug.Log("OnPause");
        if (GameEvents.CurrentState != GameState.MainGameState)
            return;
        OnPauseEvent?.Invoke();
    }
}
