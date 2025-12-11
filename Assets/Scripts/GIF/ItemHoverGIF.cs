using UnityEngine;
using UnityEngine.EventSystems;

public class ItemHoverGIF : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private ItemGIFName gifName;

    void Start()
    {
        gifName = GetComponent<ItemGIFName>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gifName != null)
            GIFManager.Instance.ShowGIF(gifName.gifName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GIFManager.Instance.HideGIF();
    }
}
