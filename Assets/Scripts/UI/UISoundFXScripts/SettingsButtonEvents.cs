using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsButtonEvents :
    MonoBehaviour,
    ISelectHandler,
    ISubmitHandler 
{
    // Fired when any button with this script is hovered/highlighted.
    public static event Action OnSettingsButtonSelected;
    public static event Action OnSettingsButtonSubmitted;

    
    public void OnSelect(BaseEventData eventData)
    {
        OnSettingsButtonSelected?.Invoke();
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        OnSettingsButtonSubmitted?.Invoke();
    }

}