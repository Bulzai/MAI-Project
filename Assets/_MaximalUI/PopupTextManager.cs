using UnityEngine;

public class PopupTextManager : MonoBehaviour
{
    public static PopupTextManager Instance;

    public GameObject popupPrefab;
    public RectTransform[] popupAnchors; // size 4

    private void Awake()
    {
        Instance = this;
    }

    public void ShowPopupForPlayer(string message, int playerIndex, Color color, bool isImportant = false)
    {
        if (popupPrefab == null) return;
        if (popupAnchors == null) return;
        if (playerIndex < 0 || playerIndex >= popupAnchors.Length) return;
        if (popupAnchors[playerIndex] == null) return;

        GameObject popupObj = Instantiate(popupPrefab, popupAnchors[playerIndex]);

        RectTransform popupRect = popupObj.GetComponent<RectTransform>();
        popupRect.anchoredPosition = Vector2.zero;
        popupRect.localScale = Vector3.one;

        PopupText popup = popupObj.GetComponent<PopupText>();
        if (popup != null)
        {
            popup.SetText(message);
            popup.SetColor(color);
            popup.isImportant = isImportant;
        }
    }
}