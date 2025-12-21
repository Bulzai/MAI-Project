using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;   

public class PlayerSelectionTextManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NotAllPlayersReadyText;
    [SerializeField] private TextMeshProUGUI NobodyJoinedYetText;

    [SerializeField] private float showDuration = 4f;

    private Coroutine _showRoutineNotAllPlayersReady;
    private Coroutine _showRoutineNobodyJoinedYet;

    private void Awake()
    {
        PlayerSelectionManager.OnNobodyJoinedYet    += HandleNobodyJoinedYet;
        PlayerSelectionManager.OnNotAllPlayersReady += HandleNotAllPlayersReady;
        GameEvents.OnPlayerSelectionStateExited     += HandlePlayerSelectionStateExited;
    }

    private void OnDestroy()
    {
        PlayerSelectionManager.OnNobodyJoinedYet    -= HandleNobodyJoinedYet;
        PlayerSelectionManager.OnNotAllPlayersReady -= HandleNotAllPlayersReady;
        GameEvents.OnPlayerSelectionStateExited     -= HandlePlayerSelectionStateExited;
    }

    private void HandlePlayerSelectionStateExited()
    {
        HideNotAllPlayersReady();
        HideNobodyJoinedYet();
    }

    private void HandleNotAllPlayersReady()
    {
        ShowNotAllPlayersReady();
    }

    private void HandleNobodyJoinedYet()
    {
        ShowNobodyJoinedYet();
    }

    // ----- NotAllPlayersReady -----

    private void ShowNotAllPlayersReady()
    {
        if (_showRoutineNotAllPlayersReady != null)
            StopCoroutine(_showRoutineNotAllPlayersReady);

        NotAllPlayersReadyText.enabled = true;
        _showRoutineNotAllPlayersReady =
            StartCoroutine(HideAfterDelay(NotAllPlayersReadyText,
                                          r => _showRoutineNotAllPlayersReady = r));
    }

    private void HideNotAllPlayersReady()
    {
        if (_showRoutineNotAllPlayersReady != null)
        {
            StopCoroutine(_showRoutineNotAllPlayersReady);
            _showRoutineNotAllPlayersReady = null;
        }

        NotAllPlayersReadyText.enabled = false;
    }

    // ----- NobodyJoinedYet -----

    private void ShowNobodyJoinedYet()
    {
        if (_showRoutineNobodyJoinedYet != null)
            StopCoroutine(_showRoutineNobodyJoinedYet);

        NobodyJoinedYetText.enabled = true;
        _showRoutineNobodyJoinedYet =
            StartCoroutine(HideAfterDelay(NobodyJoinedYetText,
                                          r => _showRoutineNobodyJoinedYet = r));
    }

    private void HideNobodyJoinedYet()
    {
        if (_showRoutineNobodyJoinedYet != null)
        {
            StopCoroutine(_showRoutineNobodyJoinedYet);
            _showRoutineNobodyJoinedYet = null;
        }

        NobodyJoinedYetText.enabled = false;
    }

    // ----- Shared coroutine -----

    private IEnumerator HideAfterDelay(TextMeshProUGUI textToHide,
                                       System.Action<Coroutine> clearRoutineField)
    {
        yield return new WaitForSeconds(showDuration);
        textToHide.enabled = false;
        clearRoutineField(null); // sets the corresponding field back to null
    }
}
