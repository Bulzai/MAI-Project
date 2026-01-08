using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuResumeButtonEvents :
    MonoBehaviour,
    ISelectHandler,
    ISubmitHandler 
{
    // Fired when any button with this script is hovered/highlighted.
    public static event Action OnPauseMenuResumeButtonSelected;
    public static event Action OnPauseMenuResumeButtonSubmitted;

    
    public void OnSelect(BaseEventData eventData)
    {
        OnPauseMenuResumeButtonSelected?.Invoke();
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        OnPauseMenuResumeButtonSubmitted?.Invoke();
    }

}