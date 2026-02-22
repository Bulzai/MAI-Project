using System;
using System.Collections;
using TMPro;
using UnityEngine;

using UnityEngine.UI;
public class SinglePlayerScoreManager : MonoBehaviour
{
    [Header("Transition")]
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private GameObject scoreboardUI;
    
    [Header("Scoreboard UI Elements")]
    [SerializeField] private TextMeshProUGUI PlayerTimeSurvivedText;
    [SerializeField] private TextMeshProUGUI PlayerMilkCollectedText;
    [SerializeField] private TextMeshProUGUI PlayerDamageFromItemsText;
    [SerializeField] private Button ContinueButton;
    [SerializeField] private Button SecondPlaythroughButton;
    [SerializeField] private Button MenuButton;
    
    [Header("Round Settings")]
    [SerializeField] private int maxRounds = 5;
    private int currentRound = 0;
    
    
    // SECOND PLAYTHROUGH
    private bool isSecondPlaythrough = false;
    
    // EVENTS
    public static event Action StartFireTransition;
    public static event Action SecondPlaythroughStarted;

    
    public static SinglePlayerScoreManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        
        PlayerHealthSystem.OnPlayerDeath += HandlePlayerDeath;
        ContinueButton.onClick.AddListener(OnContinueClicked);
        MenuButton.onClick.AddListener(OnMenuClicked);
        SecondPlaythroughButton.onClick.AddListener(OnSecondPlaythroughButtonClicked);

    }

    private void OnDestroy()
    {
        PlayerHealthSystem.OnPlayerDeath -= HandlePlayerDeath;
        ContinueButton.onClick.RemoveListener(OnContinueClicked);
        MenuButton.onClick.RemoveListener(OnMenuClicked);
    }
    
    private void IncreaseRoundsCounter()
    {
        currentRound++;
    }
    
    private void HandlePlayerDeath()
    { 
        StartCoroutine(TransitionToScoreState());
    }
    
    private IEnumerator TransitionToScoreState()
    {
        yield return new WaitForSeconds(2f);  // Adjust delay as needed
        
        GameEvents.ChangeState(GameState.ScoreState);
        StartScoreboardSequence();
    }
    
    private void StartScoreboardSequence()
    {
        IncreaseRoundsCounter();
        StartFireTransition?.Invoke();
        StopAllCoroutines();
        StartCoroutine(ScoreboardSequenceCoroutine());
    }
    
    
    private IEnumerator ScoreboardSequenceCoroutine()
    {
        yield return new WaitForSeconds(0.8f);

        // 3. UI im Hintergrund vorbereiten (während die Transition noch alles verdeckt)

        // 4. Scoreboard jetzt sichtbar machen
        PrepareScoreBoardUI();
    
        // change text values

        if (currentRound < maxRounds)
        {
            yield return new WaitForSeconds(1.5f);
            ContinueButton.gameObject.SetActive(true);
        }
        

        if (currentRound >= maxRounds && !isSecondPlaythrough)
        {
            ShowFinalScore();
            yield return new WaitForSeconds(1.5f);
            SecondPlaythroughButton.gameObject.SetActive(true);    
        }
        
        if (currentRound >= maxRounds && isSecondPlaythrough)
        {
            ShowFinalScore();
            yield return new WaitForSeconds(1.5f);
            MenuButton.gameObject.SetActive(true);
        }
        

    }
    
    private void PrepareScoreBoardUI()
    {
        PlayerTimeSurvivedText.text = "9000";

        
        if (scoreboardUI != null)
            scoreboardUI.SetActive(true);
    }

    private void ShowFinalScore()
    {
        CaclulateFinalScore();
    }
    
    private void CaclulateFinalScore()
    {
        
    }
    
    
    private void OnContinueClicked()
    {
        ContinueButton.gameObject.SetActive(false);    
        Debug.Log("Continue clicked!");
        StartFireTransition.Invoke();
        StartCoroutine(OnContinueButtonClickedCoroutine());

    }
    
    private IEnumerator OnContinueButtonClickedCoroutine()
    {
        yield return new WaitForSeconds(0.8f);
        GameEvents.ChangeState(GameState.CalculateItemPlacementState);

    }
    
    private void OnMenuClicked()
    {
        MenuButton.gameObject.SetActive(false);    
        Debug.Log("Menubutton clicked!");
        StartFireTransition.Invoke();
        StartCoroutine(OnMenuButtonClickedCoroutine());
    }
    private IEnumerator OnMenuButtonClickedCoroutine()
    {
        yield return new WaitForSeconds(0.8f);
        GameEvents.ChangeState(GameState.MenuState);

    }


    private void OnSecondPlaythroughButtonClicked()
    {
        SecondPlaythroughButton.gameObject.SetActive(false);    
        StartFireTransition.Invoke();
        StartCoroutine(OnSecondPlaythroughButtonClickedCoroutine());
        currentRound = 0;
        isSecondPlaythrough = true;
        // Start second playthrough
        // clear all data values
        // write data to file

    }

    private IEnumerator OnSecondPlaythroughButtonClickedCoroutine()
    {
        yield return new WaitForSeconds(0.8f);
        GameEvents.ChangeState(GameState.CalculateItemPlacementState);
        SecondPlaythroughStarted?.Invoke();
    }

}
