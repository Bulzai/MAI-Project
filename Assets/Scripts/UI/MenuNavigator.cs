using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuNavigator : MonoBehaviour
{
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private Selectable elementToSelect;

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

        // Clear current selection first (important for proper highlighting)
        eventSystem.SetSelectedGameObject(null);

        // Set the new selection
        eventSystem.SetSelectedGameObject(elementToSelect.gameObject);

    }
}
