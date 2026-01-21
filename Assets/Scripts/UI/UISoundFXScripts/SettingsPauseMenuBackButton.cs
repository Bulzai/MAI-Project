using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsPauseMenuBackButton :
    MonoBehaviour,
    ISelectHandler,
    ISubmitHandler
{
    // Fired when any button with this script is hovered/highlighted.
    public static event Action OnSettingsPauseMenuBackButtonSelected;
    public static event Action OnSettingsPauseMenuBackButtonSubmitted;


    public void OnSelect(BaseEventData eventData)
    {
        OnSettingsPauseMenuBackButtonSelected?.Invoke();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        OnSettingsPauseMenuBackButtonSubmitted?.Invoke();
    }
}
