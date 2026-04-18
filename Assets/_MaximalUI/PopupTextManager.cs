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

    public void ShowPopupForPlayer(string message, int playerIndex, Color color, bool isImportant = false)
    {
        if (popupPrefab == null || canvas == null) return;

        GameObject popupObj = Instantiate(popupPrefab, canvas.transform);

        RectTransform popupRect = popupObj.GetComponent<RectTransform>();
        popupRect.anchoredPosition = popupAnchors[playerIndex].anchoredPosition;

        PopupText popup = popupObj.GetComponent<PopupText>();
        if (popup != null)
        {
            popup.SetText(message);
            popup.SetColor(color);
            popup.isImportant = isImportant;
        }
    }
}