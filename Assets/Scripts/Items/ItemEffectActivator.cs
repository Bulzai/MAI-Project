using System.Collections;
using UnityEngine;

public class ItemEffectActivator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float activationDelay = 2f; // seconds
    [SerializeField] private MonoBehaviour[] scriptsToToggle; // scripts you want to enable/disable

    private void OnEnable()
    {

        GameEvents.OnMainGameStateEntered += HandleMainGameStateEntered;
        GameEvents.OnMainGameStateExited += HandleMainGameStateExited;
    }

    private void OnDisable()
    {

        GameEvents.OnMainGameStateEntered -= HandleMainGameStateEntered;
        GameEvents.OnMainGameStateExited -= HandleMainGameStateExited;
    }



    private void HandleMainGameStateEntered()
    {
        // Immediately disable all scripts
        SetScriptsEnabled(false);

        // Start coroutine to re-enable after delay
        StartCoroutine(EnableAfterDelay());
    }

    private void HandleMainGameStateExited()
    {
        StopAllCoroutines();
        SetScriptsEnabled(false);
    }

    private IEnumerator EnableAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        SetScriptsEnabled(true);
    }

    private void SetScriptsEnabled(bool enabled)
    {
        foreach (var script in scriptsToToggle)
        {
            if (script != null)
                script.enabled = enabled;
        }

    }
}
