using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayButtonTransition
    : MonoBehaviour
{
    public StateChanger _stateChanger;
    public GameObject gameMap;
    [SerializeField] private Animator transitionAnimator;
    public MainMenu mainMenu;
    private bool isPlaying = false;


    private void Awake()
    {
       
        PlayerSelectionManager.OnReturnToMainMenu += SetIsPlayingFalse;
        GameEvents.OnMenuStateEntered += SetIsPlayingFalse;
        GameEvents.OnScoreStateEntered += SetIsPlayingFalse;

    }
    private void OnDestroy()
    {
        PlayerSelectionManager.OnReturnToMainMenu -= SetIsPlayingFalse;
        GameEvents.OnMenuStateEntered -= SetIsPlayingFalse;
        GameEvents.OnScoreStateEntered -= SetIsPlayingFalse;

    }

    public void PlayFireTransitionAnimation()
    {
        Debug.Log("is playing: " + isPlaying);
        if (isPlaying) return;
        isPlaying = true;
        StartCoroutine(ExecuteTransitionThenChangeState());
    }
   
    private IEnumerator ExecuteTransitionThenChangeState()
    {
        // 1. Das Parent-Objekt finden und aktivieren
        transitionAnimator.gameObject.GetComponent<Image>().enabled = true;

        // 2. Animation Trigger setzen
        transitionAnimator.SetTrigger("Play");
        
        yield return new WaitForSeconds(1f);

        _stateChanger.GoToSelectAIState();
        gameMap.SetActive(true);

        mainMenu.PlayGame(); 
        yield return new WaitForSeconds(0.5f);
        transitionAnimator.gameObject.GetComponent<Image>().enabled = false;
        isPlaying = false;
        yield return new WaitForSeconds(0.5f);

    }
    private void SetIsPlayingFalse()
    {
        isPlaying = false;
    }
}