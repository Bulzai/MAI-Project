using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionMenuReturnButtonEvents :
    MonoBehaviour,
    ISelectHandler,
    ISubmitHandler 
{
    // Fired when any button with this script is hovered/highlighted.
    public static event Action OnOptionsMenuReturnButtonSelected;
    public static event Action OnOptionsMenuReturnButtonSubmitted;

    
    public void OnSelect(BaseEventData eventData)
    {
        OnOptionsMenuReturnButtonSelected?.Invoke();
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        OnOptionsMenuReturnButtonSubmitted?.Invoke();
    }

}