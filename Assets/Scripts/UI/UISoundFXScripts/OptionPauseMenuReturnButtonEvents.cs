using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionPauseMenuReturnButtonEvents :
    MonoBehaviour,
    ISelectHandler,
    ISubmitHandler
{
    // Fired when any button with this script is hovered/highlighted.
    public static event Action OnOptionsPauseMenuReturnButtonSelected;
    public static event Action OnOptionsPauseMenuReturnButtonSubmitted;


    public void OnSelect(BaseEventData eventData)
    {
        OnOptionsPauseMenuReturnButtonSelected?.Invoke();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        OnOptionsPauseMenuReturnButtonSubmitted?.Invoke();
    }
}
