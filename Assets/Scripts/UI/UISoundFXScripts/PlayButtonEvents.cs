using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayButtonEvents :
    MonoBehaviour,
    ISelectHandler,
    ISubmitHandler 
{
    // Fired when any button with this script is hovered/highlighted.
    public static event Action OnPlayButtonSelected;
    public static event Action OnPlayButtonSubmitted;

    
    public void OnSelect(BaseEventData eventData)
    {
        OnPlayButtonSelected?.Invoke();
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        OnPlayButtonSubmitted?.Invoke();
    }

}