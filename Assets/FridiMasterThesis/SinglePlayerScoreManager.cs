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
    [SerializeField] private GameObject rows;
    [Header("Scoreboard UI Elements")]
    [SerializeField] private TextMeshProUGUI PlayerDamageFromItemsText;
    [SerializeField] private Button ContinueButton;
    [SerializeField] private Button NextPlaythroughButton;
    [SerializeField] private Button MenuButton;
    [SerializeField] private Button SmartPlacementQuestionnaireButton;
    [SerializeField] private Button RandomPlacementQuestionnaireButton;
    [SerializeField] private Button OpenPlaytestDataFolderButton;
    [SerializeField] private TextMeshProUGUI[] MilkCollectedScores;
    [SerializeField] private TextMeshProUGUI totalMilkCollectedText;
    [SerializeField] private TMP_Text textToCopyText;
    [SerializeField] private GameObject TextToCopyGO;
    [SerializeField] private TMP_Text CopiedFeedbackText;

    [Header("Round Settings")]
    public int MAX_ROUNDS = 2;
    
    
    // EVENTS
    public static event Action StartFireTransition;
    public static event Action StartSlowFireTransition;
    public static event Action OnPlaythroughStarted;
    public static event Action OnPrepareNextPlaythrough;
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
    

    
    private void HandlePlayerDeath()
    { 
        StartCoroutine(TransitionToScoreState());
    }
    
    public void Handle10MilkCartonsCollected()
    {
        TransitionToScoreStateNoPause();
    }
    
    private IEnumerator TransitionToScoreState()
    {
        yield return new WaitForSeconds(1.0f);  // Adjust delay as needed
        
        GameEvents.ChangeState(GameState.ScoreState);
        StartScoreboardSequence();
    }
    
    private void TransitionToScoreStateNoPause()
    {
        GameEvents.ChangeState(GameState.ScoreState);
        StartScoreboardSequence();
    }
    
    private void StartScoreboardSequence()
    {
        PlayTestDataManager.Instance.roundIndex.Increment();
        Debug.Log("Round index incremented to: " + PlayTestDataManager.Instance.roundIndex.Value);
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

        if (PlayTestDataManager.Instance.roundIndex.Value < MAX_ROUNDS)
        {
            yield return new WaitForSeconds(2.5f);
            ContinueButton.gameObject.SetActive(true);
            PlayTestDataManager.Instance.LogRoundScore();
        } 
        else
        {
            PlayTestDataManager.Instance.previousGamesPlayed++;
            PlayTestDataManager.Instance.SavePreviousGamesPlayed();
            PlayTestDataManager.Instance.LogRoundScore();
            PlayTestDataManager.Instance.LogTotalScore();
            ShowCorrectQuestionnaireButton();
            PlayTestDataManager.Instance.ResetAllCounters();
            RandomPlacementStrategy.Instance.ClearItemParent();
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
            case PlaythroughType.A:
                RandomPlacementQuestionnaireButton.gameObject.SetActive(true);
                break;
            case PlaythroughType.B:
                SmartPlacementQuestionnaireButton.gameObject.SetActive(true);
                break;
            default:
                Debug.LogError("Unhandled playthrough type!");
                break;
        }
        
        textToCopyText.text = "userId " + PlayTestDataManager.Instance.GetUserID() + "\n" +
                              "sessionId " + PlayTestDataManager.Instance.GetSessionID() + "\n" +
                              "previousGamesPlayed " + PlayTestDataManager.Instance.previousGamesPlayed;
        TextToCopyGO.SetActive(true);

    }
    
    public void AfterQuestionnaireButtonWasClicked()
    {
        CopiedFeedbackText.gameObject.SetActive(false);

        RandomPlacementQuestionnaireButton.gameObject.SetActive(false);
        SmartPlacementQuestionnaireButton.gameObject.SetActive(false);

        StartCoroutine(AfterQuestionnaireDelayed());
    }

    private IEnumerator AfterQuestionnaireDelayed()
    {
        yield return new WaitForSeconds(1f);
    
        if (PlayTestDataManager.Instance.previousGamesPlayed > 1)
            OpenPlaytestDataFolderButton.gameObject.SetActive(true);
        else
        {
            NextPlaythroughButton.gameObject.SetActive(true);
        }
    }
    public void AfterOpenPlaytestFolderButtonClicked()
    {
        OpenPlaytestDataFolderButton.gameObject.SetActive(false);
        //MenuButton.gameObject.SetActive(true);
        NextPlaythroughButton.gameObject.SetActive(true);
    }

    private void PrepareScoreBoardUI()
    {
        int currentRoundIndex = PlayTestDataManager.Instance.roundIndex.Value;
        for (int i = 0; i < PlayTestDataManager.Instance.roundIndex.Value; i++)
        {
            rows.transform.GetChild(i).gameObject.SetActive(true);
        }
        
        MilkCollectedScores[currentRoundIndex - 1].text = PlayTestDataManager.Instance.roundMilkCollected.Value.ToString();

        if (currentRoundIndex == MAX_ROUNDS)
        {
            rows.transform.GetChild(currentRoundIndex).gameObject.SetActive(true);
            totalMilkCollectedText.text = PlayTestDataManager.Instance.totalMilkCollected.Value.ToString();
        }
        if (scoreboardUI != null)
            scoreboardUI.SetActive(true);
    }
    
    
    private void OnContinueClicked()
    {
        ContinueButton.gameObject.SetActive(false);    
        Debug.Log("Continue clicked!");
        StartCoroutine(OnContinueButtonClickedCoroutine());
        StartSlowFireTransition.Invoke();
    }
    
    private IEnumerator OnContinueButtonClickedCoroutine()
    {
        yield return new WaitForSeconds(0.8f);
        HideScoreBoard();
        OnNextRoundStarted?.Invoke();
    }
    
    private void HideScoreBoard()
    {
        for (int i = 0; i < rows.transform.childCount; i++)
        {
            rows.transform.GetChild(i).gameObject.SetActive(false);
        }
        scoreboardUI.SetActive(false);
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
        textToCopyText.gameObject.SetActive(false);

        HideAllButtons();
        StartFireTransition.Invoke();
        StartCoroutine(OnNextPlaythroughButtonClickedCoroutine());

    }

    private IEnumerator OnNextPlaythroughButtonClickedCoroutine()
    {
        OnPrepareNextPlaythrough?.Invoke();
        yield return new WaitForSeconds(0.8f);
        HideScoreBoard();
        OnPlaythroughStarted?.Invoke();
        
    }
    
    
    private void HideAllButtons()
    {
        TextToCopyGO.SetActive(false);
        ContinueButton.gameObject.SetActive(false);
        MenuButton.gameObject.SetActive(false);
        NextPlaythroughButton.gameObject.SetActive(false);
        SmartPlacementQuestionnaireButton.gameObject.SetActive(false);
        RandomPlacementQuestionnaireButton.gameObject.SetActive(false);
    }
}
