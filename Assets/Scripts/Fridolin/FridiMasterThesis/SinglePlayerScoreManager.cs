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
    [SerializeField] private Button NextPlaythroughButton;
    [SerializeField] private Button MenuButton;
    [SerializeField] private Button SmartPlacementQuestionnaireButton;
    [SerializeField] private Button RandomPlacementQuestionnaireButton;
    
    [Header("Round Settings")]
    [SerializeField] private int maxRounds = 5;
    private int currentRound = 0;
    
    
    // EVENTS
    public static event Action StartFireTransition;
    public static event Action OnPlaythroughStarted;
    public static event Action OnNextRoundStarted;
    
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
        NextPlaythroughButton.onClick.AddListener(OnNextPlaythroughButtonClicked);
        
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
            PlayTestDataManager.Instance.LogRoundScore();
        } 
        else
        {
            PlayTestDataManager.Instance.LogRoundScore();
            PlayTestDataManager.Instance.LogTotalScore();
            ShowCorrectQuestionnaireButton();
            ShowFinalScore();
        } 
    }

    public void ShowCorrectQuestionnaireButton()
    { 
        PlaythroughType playthroughType = AISelector.Instance.currentPlaythroughType;
        switch (playthroughType)
        {
            case PlaythroughType.Undefined:
                Debug.LogError("Playthrough type is undefined!");
                break;
            case PlaythroughType.VersionA:
                RandomPlacementQuestionnaireButton.gameObject.SetActive(true);
                break;
            case PlaythroughType.VersionB:
                SmartPlacementQuestionnaireButton.gameObject.SetActive(true);
                break;
            default:
                Debug.LogError("Unhandled playthrough type!");
                break;
        }
        RandomPlacementQuestionnaireButton.gameObject.SetActive(false);
        SmartPlacementQuestionnaireButton.gameObject.SetActive(false);
        
        ShowMenuAndNextPlaythroughButtons();
        

    }
    
    public void ShowMenuAndNextPlaythroughButtons()
    {
        MenuButton.gameObject.SetActive(true);
        NextPlaythroughButton.gameObject.SetActive(true);
    }
    
    private void PrepareScoreBoardUI()
    {
        PlayerTimeSurvivedText.text = "9000";

        SendDummyDataToPlayTestDataManager();
        PlayTestDataManager.Instance.SavePreviousGamesPlayed();
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
        PlayTestDataManager.Instance.LogRoundScore();
    }
    
    private IEnumerator OnContinueButtonClickedCoroutine()
    {
        yield return new WaitForSeconds(0.8f);
        GameEvents.ChangeState(GameState.SelectAIState);

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
        HideAllButtons();
        yield return new WaitForSeconds(0.8f);
        GameEvents.ChangeState(GameState.MenuState);

    }


    private void OnNextPlaythroughButtonClicked()
    {
        HideAllButtons();
        StartFireTransition.Invoke();
        StartCoroutine(OnNextPlaythroughButtonClickedCoroutine());
        currentRound = 0;

    }

    private IEnumerator OnNextPlaythroughButtonClickedCoroutine()
    {
        yield return new WaitForSeconds(0.8f);
        OnPlaythroughStarted?.Invoke();
        
    }

    private void SendDummyDataToPlayTestDataManager()
    {
        PlayTestDataManager.Instance?.LogRoundScore();
        Debug.Log("Logged dummy RoundScore event!");
    }
    
    
    private void HideAllButtons()
    {
        ContinueButton.gameObject.SetActive(false);
        MenuButton.gameObject.SetActive(false);
        NextPlaythroughButton.gameObject.SetActive(false);
        SmartPlacementQuestionnaireButton.gameObject.SetActive(false);
        RandomPlacementQuestionnaireButton.gameObject.SetActive(false);
    }
}
