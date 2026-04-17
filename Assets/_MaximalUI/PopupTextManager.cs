using UnityEngine;

public class PopupTextManager : MonoBehaviour
{
    public static PopupTextManager Instance;

    public GameObject popupPrefab;
    public Canvas canvas;
    public RectTransform[] popupAnchors; // size 4

    private void Awake()
    {
        Instance = this;
    }

    public void ShowPopupForPlayer(string message, int playerIndex)
    {
        if (popupPrefab == null || canvas == null)
        {
            Debug.LogWarning("Popup prefab or canvas not assigned.");
            return;
        }

        if (playerIndex < 0 || playerIndex >= popupAnchors.Length || popupAnchors[playerIndex] == null)
        {
            Debug.LogWarning("Popup anchor missing for player index: " + playerIndex);
            return;
        }

        GameObject popupObj = Instantiate(popupPrefab, canvas.transform);

        RectTransform popupRect = popupObj.GetComponent<RectTransform>();
        popupRect.anchoredPosition = popupAnchors[playerIndex].anchoredPosition;

        PopupText popup = popupObj.GetComponent<PopupText>();
        if (popup != null)
        {
            popup.SetText(message);
        }
    }
}