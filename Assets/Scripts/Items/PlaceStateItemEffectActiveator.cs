using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceStateItemEffectActiveator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float activationDelay = 2f; // seconds
    [SerializeField] private MonoBehaviour[] scriptsToToggle; // scripts you want to enable/disable
    public Animator animToToggle;
    private void OnEnable()
    {

        GameEvents.OnPlaceItemStateEntered += HandlePlaceGameStateEntered;
       
    }

    private void OnDisable()
    {

        GameEvents.OnPlaceItemStateEntered -= HandlePlaceGameStateEntered;
    }



    private void HandlePlaceGameStateEntered()
    {
        // Immediately disable all scripts
        SetScriptsEnabled(false);

        Invoke("AnimatorActive", 1f);
        Debug.Log("Placeitementeededed");
        // Start coroutine to re-enable after delay
        StartCoroutine(EnableAfterDelay());
    }
    void AnimatorActive()
    {
        if (animToToggle != null)
        {
            animToToggle.enabled = true;
            Debug.Log("anim aktivated");
        }
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
