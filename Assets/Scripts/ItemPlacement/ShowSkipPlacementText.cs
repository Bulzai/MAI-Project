using TMPro;
using UnityEngine;

public class ShowSkipPlacementText : MonoBehaviour
{
    private TextMeshProUGUI tmpText;

    private void Awake()
    {
        // Cache the component once (better than calling GetComponent repeatedly)
        tmpText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlaceItemStateEntered += ShowText;
        GameEvents.OnMainGameStateEntered += HideText;
    }

    private void OnDisable()
    {
        GameEvents.OnPlaceItemStateEntered -= ShowText;
        GameEvents.OnMainGameStateEntered -= HideText;
    }

    private void ShowText()
    {
        tmpText.enabled = true;
    }

    private void HideText()
    {
        tmpText.enabled = false;
    }
}
