using UnityEngine;

public class ShowSkipPlacementText : MonoBehaviour
{
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
        gameObject.SetActive(true);
    }

    private void HideText()
    {
        gameObject.SetActive(false);
    }
}
