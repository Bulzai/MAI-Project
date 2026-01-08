using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuMenuButtonEvents :
    MonoBehaviour,
    ISelectHandler,
    ISubmitHandler 
{
    // Fired when any button with this script is hovered/highlighted.
    public static event Action OnPauseMenuMenuButtonSelected;
    public static event Action OnPauseMenuMenuButtonSubmitted;

    
    public void OnSelect(BaseEventData eventData)
    {
        OnPauseMenuMenuButtonSelected?.Invoke();
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        OnPauseMenuMenuButtonSubmitted?.Invoke();
    }
}