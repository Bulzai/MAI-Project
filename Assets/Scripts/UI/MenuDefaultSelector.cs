using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuDefaultSelector : MonoBehaviour
{
    [SerializeField] private Selectable defaultSelectable;

    private void OnEnable()
    {
        if (defaultSelectable == null)
        {
            Debug.LogWarning("No default selectable assigned on " + name, this);
            return;
        }

        // Sicherstellen, dass ein EventSystem existiert
        var eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogWarning("No EventSystem in scene.", this);
            return;
        }

        // Wichtig: erst auf null setzen, dann neu
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(defaultSelectable.gameObject);
    }
}
