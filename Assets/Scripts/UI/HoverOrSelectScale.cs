using UnityEngine;
using UnityEngine.EventSystems;

public class HoverOrSelectScale : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Scale Settings")]
    public float scaleMultiplier = 1.1f; // 1.1 = +10%, 1.2 = +20%

    public float scaleSpeed = 10f;

    private Vector3 _normalScale;
    private Vector3 _targetScale;

    private bool _isPointerOver = false;
    private bool _isSelected = false;

    void Awake()
    {
        _normalScale = transform.localScale;
        _targetScale = _normalScale * scaleMultiplier;
    }

    void Update()
    {
        bool isHighlighted = _isPointerOver || _isSelected;
        Vector3 desired = isHighlighted ? _targetScale : _normalScale;

        transform.localScale = Vector3.Lerp(transform.localScale, desired, Time.deltaTime * scaleSpeed);
    }

    // --- Mouse Hover ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
    }

    // --- Controller / Keyboard Selection ---
    public void OnSelect(BaseEventData eventData)
    {
        _isSelected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _isSelected = false;
    }
}
