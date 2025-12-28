using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonSelector: MonoBehaviour
{
    private EventSystem eventSystem;
    private Selectable elementToSelect;

    private void Awake()
    {
        // Auto-assign EventSystem if not set
        if (eventSystem == null)
            eventSystem = EventSystem.current;

        // Auto-assign the Selectable on this GameObject if not set
        if (elementToSelect == null)
            elementToSelect = GetComponent<Selectable>();
        //ScoreboardRowUI.OnSoreboardAnimationFinished += JumpToElement;
    }
    
    private void OnEnable()
    {
        // When this object becomes active, jump to its own selectable
        if (eventSystem == null || elementToSelect == null) return;

        StartCoroutine(EnableMenuButtonAfterDelay());

        
    }

    private IEnumerator EnableMenuButtonAfterDelay()
    {
        yield return new WaitForSeconds(4);
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(elementToSelect.gameObject);
    }

    public void JumpToElement()
    {
        if (eventSystem == null)
        {
            Debug.LogWarning("No EventSystem referenced.", this);
            return;
        }

        if (elementToSelect == null)
        {
            Debug.LogWarning("No element assigned to jump to.", this);
            return;
        }

        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(elementToSelect.gameObject);

        Debug.Log($"Jumped to: {elementToSelect.name}", this);
    }
}