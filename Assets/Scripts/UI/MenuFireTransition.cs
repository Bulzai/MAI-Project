using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuFireTransition : MonoBehaviour
{
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private IntroStoryScreen introStoryScreen;

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
        StartCoroutine(ExecuteTransitionThenShowIntro());
    }

    private IEnumerator ExecuteTransitionThenShowIntro()
    {
        Image transitionImage = transitionAnimator.GetComponent<Image>();
        transitionImage.enabled = true;

        transitionAnimator.SetTrigger("Play");

        yield return new WaitForSeconds(1f);

        // Hide main menu
        if (mainMenu != null)
            mainMenu.PlayGame();

        // Show intro instead of going directly to player selection
        if (introStoryScreen != null)
            introStoryScreen.StartIntro();

        yield return new WaitForSeconds(0.5f);

        transitionImage.enabled = false;
        isPlaying = false;
    }

    private void SetIsPlayingFalse()
    {
        isPlaying = false;
    }
}