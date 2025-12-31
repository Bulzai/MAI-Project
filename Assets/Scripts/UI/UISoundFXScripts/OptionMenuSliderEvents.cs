using System;
using UnityEngine;

public class OptionMenuSliderEvents : MonoBehaviour
{
    private float lastValue;

    public static event Action OnSliderSlideLeft;
    public static event Action OnSliderSlideRight;

    public void OnSliderChanged(float newValue)
    {
        if (Mathf.Approximately(newValue, lastValue))
            return;

        if (newValue > lastValue)
        {
            OnSliderSlideRight?.Invoke();
        }
        else
        {
            OnSliderSlideLeft?.Invoke();
        }

        lastValue = newValue;
    }
}
