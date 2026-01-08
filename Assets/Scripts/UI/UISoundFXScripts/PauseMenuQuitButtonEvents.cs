using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuQuitButtonEvents :
    MonoBehaviour,
    ISelectHandler,
    ISubmitHandler 
{
    // Fired when any button with this script is hovered/highlighted.
    public static event Action OnPauseMenuQuitButtonSelected;
    public static event Action OnPauseMenuQuitButtonSubmitted;

    
    public void OnSelect(BaseEventData eventData)
    {
        OnPauseMenuQuitButtonSelected?.Invoke();
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        OnPauseMenuQuitButtonSubmitted?.Invoke();
    }

}