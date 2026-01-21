using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuSettingsButtonEvents :
    MonoBehaviour,
    ISelectHandler,
    ISubmitHandler
{
    // Fired when any button with this script is hovered/highlighted.
    public static event Action OnPauseMenuSettingsButtonSelected;
    public static event Action OnPauseMenuSettingsButtonSubmitted;


    public void OnSelect(BaseEventData eventData)
    {
        OnPauseMenuSettingsButtonSelected?.Invoke();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        OnPauseMenuSettingsButtonSubmitted?.Invoke();
    }
}
