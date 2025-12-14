using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowInGameUI : MonoBehaviour
{
    private TextMeshProUGUI tmpText;
    private Image image;

    private void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlaceItemStateEntered += Show;
        GameEvents.OnMainGameStateEntered += Hide;
    }

    private void OnDisable()
    {
        GameEvents.OnPlaceItemStateEntered -= Show;
        GameEvents.OnMainGameStateEntered -= Hide;
    }

    private void Show()
    {
        if (tmpText != null)
            tmpText.enabled = true;

        if (image != null)
            image.enabled = true;
    }

    private void Hide()
    {
        if (tmpText != null)
            tmpText.enabled = false;

        if (image != null)
            image.enabled = false;
    }
}
