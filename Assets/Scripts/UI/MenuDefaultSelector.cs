//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public class MenuDefaultSelector : MonoBehaviour
//{
//    [SerializeField] private Selectable defaultSelectable;

//    private void OnEnable()
//    {
//        if (defaultSelectable == null)
//        {
//            Debug.LogWarning("No default selectable assigned on " + name, this);
//            return;
//        }

//        // Sicherstellen, dass ein EventSystem existiert
//        var eventSystem = EventSystem.current;
//        if (eventSystem == null)
//        {
//            Debug.LogWarning("No EventSystem in scene.", this);
//            return;
//        }

//        // Wichtig: erst auf null setzen, dann neu
//        eventSystem.SetSelectedGameObject(null);
//        eventSystem.SetSelectedGameObject(defaultSelectable.gameObject);
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuDefaultSelector : MonoBehaviour
{
    [SerializeField] private Selectable defaultSelectable;
    [SerializeField] private ItemDisplayPoolUIGenerator dynamicSelectableProvider;

    private void OnEnable()
    {
        StartCoroutine(SelectRoutine());
    }

    private IEnumerator SelectRoutine()
    {
        yield return new WaitForEndOfFrame();
        yield return null;

        var eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogWarning("No EventSystem in scene.", this);
            yield break;
        }

        Selectable target = null;

        if (dynamicSelectableProvider != null)
        {
            target = dynamicSelectableProvider.GetDefaultSelectable();
        }

        if (target == null)
        {
            target = defaultSelectable;
        }

        if (target == null)
        {
            Debug.LogWarning("No default selectable assigned on " + name, this);
            yield break;
        }

        if (!target.gameObject.activeInHierarchy || !target.IsInteractable())
        {
            Debug.LogWarning("Target is not selectable: " + target.name, this);
            yield break;
        }

        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(target.gameObject);
    }
}