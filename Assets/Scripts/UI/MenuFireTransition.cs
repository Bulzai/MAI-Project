using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuFireTransition : MonoBehaviour
{
   public StateChanger _stateChanger;
   public GameObject playerSelectionGameObject;
   [SerializeField] private Animator transitionAnimator;
   public MainMenu mainMenu;
   private bool isPlaying = false;
   
   public void PlayFireTransitionAnimation()
   {
       if (isPlaying) return;
       isPlaying = true;
       StartCoroutine(ExecuteTransitionThenChangeState());
       isPlaying = false;

   }
   
   private IEnumerator ExecuteTransitionThenChangeState()
   {
      // SupriseBoxState.OnFireTransitionAnimationStarted?.Invoke();
       // 1. Das Parent-Objekt finden und aktivieren
       transitionAnimator.gameObject.GetComponent<Image>().enabled = true;

       // 2. Animation Trigger setzen
       transitionAnimator.SetTrigger("Play");
        
       yield return new WaitForSeconds(1f);

       playerSelectionGameObject.SetActive(true);
       _stateChanger.GoToPlayerSelectState();
       mainMenu.PlayGame(); 
       yield return new WaitForSeconds(0.5f);
       transitionAnimator.gameObject.GetComponent<Image>().enabled = false;
       isPlaying = false;
   }
}
