using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    [Header("Transition")]
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private CircleLoadAnim circleLoadAnim;
    [SerializeField] private GameObject circleAnimGO;
    public static TransitionManager Instance { get; private set; }

    public static event Action OnTransitionStarted;
    public static bool placementStrategyStillGoing = true;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        SinglePlayerScoreManager.StartFireTransition += StartFireTransitionAnimation;
        SinglePlayerScoreManager.StartSlowFireTransition += StartSlowFireTransitionAnimation;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SinglePlayerScoreManager.StartFireTransition -= StartFireTransitionAnimation;
            SinglePlayerScoreManager.StartSlowFireTransition -= StartSlowFireTransitionAnimation;
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
        StartCoroutine(FireTransitionAnimationCoroutine());
    }
    
    private IEnumerator SlowFireTransitionAnimationCoroutine()
    {
        // 1. Transition vorbereiten & starten
        Image transitionImage = transitionAnimator.GetComponent<Image>();
        transitionImage.enabled = true;
        transitionAnimator.SetTrigger("Play");
        yield return new WaitForSeconds(.7f);
        transitionAnimator.enabled = false;
        circleAnimGO.SetActive(true);
        CircleLoadAnim.playAnim = true;
        circleLoadAnim.StartCircleAnim();
        while (CircleLoadAnim.playAnim)
        {
            Debug.Log("CircleLoadAnim.playAnim: " + CircleLoadAnim.playAnim);
            yield return null;
        }
        circleAnimGO.SetActive(false);

        transitionAnimator.enabled = true;
        yield return new WaitForSeconds(0.6f);
        transitionImage.enabled = false;

    }
    
    private void StartSlowFireTransitionAnimation()
    {
        StartCoroutine(SlowFireTransitionAnimationCoroutine());
    }
}
