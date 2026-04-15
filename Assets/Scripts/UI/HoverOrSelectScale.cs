using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    private Toggle _toggle;

    public GameObject gifObject;

    void Awake()
    {
        _originalScale = transform.localScale;

        _toggle = GetComponentInChildren<Toggle>();
    }

    void OnEnable()
    {
        _isPointerOver = false;
        _isSelected = false;
        _pulseTime = 0f;
        transform.localScale = _originalScale;

        if (gifObject != null) gifObject.SetActive(false);
    }

    void Update()
    {
        bool shouldPulse = false;
        bool isToggledOn = (_toggle != null && _toggle.isOn);
        Image img = GetComponentInChildren<Image>();

        // Advance local pulse timer only when selected
        // add logic for toggle
        bool childIsSelected = false;
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            // check if the currently selected thing is this object or one of its children
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            childIsSelected = (selected == gameObject || selected.transform.IsChildOf(transform));
        }

        shouldPulse = _isSelected || _isPointerOver || childIsSelected;

        if (gifObject != null)
        {
            gifObject.SetActive(shouldPulse);
        }

        Vector3 targetScale;
        if (shouldPulse)
        {
            _pulseTime += Time.deltaTime;

            // Consistent pulse starting from select moment
            float pulse = Mathf.Sin(_pulseTime * pulseSpeed * Mathf.PI * 2f);
            float pulseMultiplier = Mathf.Lerp(selectMultiplierMin, selectMultiplierMax, (pulse + 1f) / 2f);
            targetScale = _originalScale * pulseMultiplier;

            if (_toggle != null && GameEvents.CurrentState == GameState.ItemDisplay)
            {
                if (img != null)
                {
                    //
                }
            }

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
