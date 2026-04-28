using TMPro;
using UnityEngine;

public class ControlsTextVisibility : MonoBehaviour
{
    [SerializeField] private TMP_Text controlsText;

    private void Awake()
    {
        Hide();
    }

    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += Show;
        GameEvents.OnMainGameStateExited += Hide;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= Show;
        GameEvents.OnMainGameStateExited -= Hide;
    }

    private void Show()
    {
        if (controlsText != null)
            controlsText.enabled = true;
    }

    private void Hide()
    {
        if (controlsText != null)
            controlsText.enabled = false;
    }
}