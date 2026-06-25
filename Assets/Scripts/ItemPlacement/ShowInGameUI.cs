using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShowInGameUI : MonoBehaviour
{
    private TextMeshProUGUI tmpText;
    private Image image;
    [SerializeField] private List<GameObject> placeItemButtonIcons;

    private void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlaceItemStateEntered += Show;
        PlaceItemState.OnGuideScrollOpen += Hide;
        GameEvents.OnMainGameStateEntered += Hide;
    }

    private void OnDisable()
    {
        GameEvents.OnPlaceItemStateEntered -= Show;
        GameEvents.OnMainGameStateEntered -= Hide;
        PlaceItemState.OnGuideScrollOpen -= Hide;

    }

    private void Show()
    {

        if (tmpText != null)
            tmpText.enabled = true;

        if (image != null)
            image.enabled = true;

        foreach (GameObject item in placeItemButtonIcons)
        {
            if (item != null)
            {
                item.SetActive(true);
            }
        }
    }

    private void Hide()
    {
        if (tmpText != null)
            tmpText.enabled = false;

        if (image != null)
            image.enabled = false;

        foreach (GameObject item in placeItemButtonIcons)
        {
            if (item != null)
            {
                item.SetActive(false);
            }
        }
    }
}
