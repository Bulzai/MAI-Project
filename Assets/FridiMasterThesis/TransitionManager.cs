using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    [Header("Transition")]
    [SerializeField] private Animator transitionAnimator;
    public static TransitionManager Instance { get; private set; }

    public static event Action OnTransitionStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        SinglePlayerScoreManager.StartFireTransition += StartFireTransitionAnimation;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SinglePlayerScoreManager.StartFireTransition -= StartFireTransitionAnimation;
            Instance = null;
        }
    }
    
    private IEnumerator FireTransitionAnimationCoroutine()
    {
        // 1. Transition vorbereiten & starten
        Image transitionImage = transitionAnimator.GetComponent<Image>();
        transitionImage.enabled = true;
        transitionAnimator.SetTrigger("Play");
        yield return new WaitForSeconds(1.3f);
        transitionImage.enabled = false;

    }
    
    private void StartFireTransitionAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(FireTransitionAnimationCoroutine());
    }
}
