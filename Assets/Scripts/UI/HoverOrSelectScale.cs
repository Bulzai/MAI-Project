using UnityEngine;
using UnityEngine.EventSystems;

public class HoverOrSelectScale : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Scale Settings")]
    public float hoverMultiplier = 1.1f;
    public float selectMultiplierMin = 1.15f;
    public float selectMultiplierMax = 1.25f;
    public float pulseSpeed = 2f;
    public float scaleSpeed = 10f;

    private Vector3 _originalScale;
    private bool _isPointerOver = false;
    private bool _isSelected = false;
    private float _pulseTime = 0f; // Local pulse timer

    void Awake()
    {
        _originalScale = transform.localScale;
    }

    void OnEnable()
    {
        _isPointerOver = false;
        _isSelected = false;
        _pulseTime = 0f;
        transform.localScale = _originalScale;
    }

    void Update()
    {
        // Advance local pulse timer only when selected
        if (_isSelected) _pulseTime += Time.deltaTime;

        Vector3 targetScale;
        if (_isSelected)
        {
            // Consistent pulse starting from select moment
            float pulse = Mathf.Sin(_pulseTime * pulseSpeed * Mathf.PI * 2f);
            float pulseMultiplier = Mathf.Lerp(selectMultiplierMin, selectMultiplierMax, (pulse + 1f) / 2f);
            targetScale = _originalScale * pulseMultiplier;
        }
        else if (_isPointerOver)
        {
            targetScale = _originalScale * hoverMultiplier;
        }
        else
        {
            targetScale = _originalScale;
        }

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData) => _isPointerOver = true;
    public void OnPointerExit(PointerEventData eventData) => _isPointerOver = false;

    public void OnSelect(BaseEventData eventData)
    {
        _isSelected = true;
        _pulseTime = 0f; // Reset pulse phase for consistent start
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _isSelected = false;
        _pulseTime = 0f;
    }
}
