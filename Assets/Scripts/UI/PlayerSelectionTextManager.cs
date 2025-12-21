using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;   

public class PlayerSelectionTextManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NotAllPlayersReadyText;
    [SerializeField] private TextMeshProUGUI NobodyJoinedYetText;

    [SerializeField] private float showDuration = 4f;

    private Coroutine _showRoutine;

    private void Awake()
    {
        PlayerSelectionManager.OnNobodyJoinedYet += HandleNobodyJoinedYet;
        PlayerSelectionManager.OnNotAllPlayersReady += HandleNotAllPlayersReady;
        GameEvents.OnPlayerSelectionStateExited += HandlePlayerSelectionStateExited;
    }
    
    private void OnDestroy()
    {
        PlayerSelectionManager.OnNobodyJoinedYet  -= HandleNobodyJoinedYet;
        PlayerSelectionManager.OnNotAllPlayersReady -= HandleNotAllPlayersReady;
        GameEvents.OnPlayerSelectionStateExited -= HandlePlayerSelectionStateExited;
    }

    private void HandlePlayerSelectionStateExited()
    {
        HideNow(NotAllPlayersReadyText);
        HideNow(NobodyJoinedYetText);
    }
    
    private void HandleNotAllPlayersReady()
    {
        ShowForSeconds(NotAllPlayersReadyText);
    }
    
    private void HandleNobodyJoinedYet()
    {
        ShowForSeconds(NobodyJoinedYetText);
    }
    
    // Call this to show for a few seconds
    public void ShowForSeconds(TextMeshProUGUI textToShow)
    {
        if (_showRoutine != null)
            StopCoroutine(_showRoutine);

        textToShow.enabled = true;
        _showRoutine = StartCoroutine(HideAfterDelay(textToShow));
    }

    // Call this to hide immediately
    public void HideNow(TextMeshProUGUI textToHide)
    {
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }

        textToHide.enabled = false;
    }

    private IEnumerator HideAfterDelay(TextMeshProUGUI textToHide)
    {
        yield return new WaitForSeconds(showDuration);
        textToHide.enabled = false;
        _showRoutine = null;
    }
}
