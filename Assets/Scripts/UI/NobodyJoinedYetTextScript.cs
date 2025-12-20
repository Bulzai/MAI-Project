using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;   

public class NobodyJoinedYetTextScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textToShow;
    [SerializeField] private float showDuration = 4f;

    private Coroutine _showRoutine;

    private void Awake()
    {
        PlayerSelectionManager.OnNobodyJoinedYet += ShowForSeconds;
        GameEvents.OnPlayerSelectionStateExited += HideNow;
    }
    
    private void OnDestroy()
    {
        PlayerSelectionManager.OnNotAllPlayersReady -= ShowForSeconds;
        GameEvents.OnPlayerSelectionStateExited -= HideNow;
    }
    
    
    // Call this to show for a few seconds
    public void ShowForSeconds()
    {
        if (_showRoutine != null)
            StopCoroutine(_showRoutine);

        textToShow.gameObject.SetActive(true);
        _showRoutine = StartCoroutine(HideAfterDelay());
    }

    // Call this to hide immediately
    public void HideNow()
    {
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }

        textToShow.gameObject.SetActive(false);
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);
        textToShow.gameObject.SetActive(false);
        _showRoutine = null;
    }
}
