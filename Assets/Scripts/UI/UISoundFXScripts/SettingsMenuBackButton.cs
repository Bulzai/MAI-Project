using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenuBackButton :
    MonoBehaviour,
    ISelectHandler,
    ISubmitHandler 
{
    // Fired when any button with this script is hovered/highlighted.
    public static event Action OnSettingsMenuBackButtonSelected;
    public static event Action OnSettingsMenuBackButtonSubmitted;

    
    public void OnSelect(BaseEventData eventData)
    {
        OnSettingsMenuBackButtonSelected?.Invoke();
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        OnSettingsMenuBackButtonSubmitted?.Invoke();
    }

}