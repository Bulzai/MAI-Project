using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuitButtonEvents :
    MonoBehaviour,
    ISelectHandler,
    ISubmitHandler 
{
    // Fired when any button with this script is hovered/highlighted.
    public static event Action OnQuitButtonSelected;
    public static event Action OnQuitButtonSubmitted;

    
    public void OnSelect(BaseEventData eventData)
    {
        OnQuitButtonSelected?.Invoke();
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        OnQuitButtonSubmitted?.Invoke();
    }

}