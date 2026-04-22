using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonSelector : MonoBehaviour
{
    private EventSystem eventSystem;
    private Selectable elementToSelect;

    private void Awake()
    {
        if (eventSystem == null)
            eventSystem = EventSystem.current;

        if (elementToSelect == null)
            elementToSelect = GetComponent<Selectable>();
    }

    private void OnEnable()
    {
        StartCoroutine(SelectNextFrame());
    }

    private IEnumerator SelectNextFrame()
    {
        yield return null; // wait 1 frame

        if (eventSystem == null)
            eventSystem = EventSystem.current;

        if (eventSystem == null || elementToSelect == null)
            yield break;

        eventSystem.SetSelectedGameObject(null);
        yield return null; // optional second frame, helps sometimes
        eventSystem.SetSelectedGameObject(elementToSelect.gameObject);
    }

    public void JumpToElement()
    {
        StartCoroutine(SelectNextFrame());
    }
}