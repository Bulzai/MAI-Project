using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Wichtig für das Image-Component

public class RoundController : MonoBehaviour
{
    public static event System.Action OnScoreboardTransitionStarted;
    [Header("Round Settings")]
    [SerializeField] private int maxRounds = 3;
    [SerializeField] private float scoreDisplayTime = 5f;

    [Header("Transition")]
    [SerializeField] private Animator transitionAnimator; // Hier den Animator zuweisen

    [SerializeField] private GameObject scoreboardView;
    [SerializeField] private GameObject questionnaireView;

    [SerializeField] private GameObject questionnairePanel;

    public GameObject EndScoreText;
    public PlayerManager playerManagerFinal;
    private PlayerScoreManager playerScoreManager;

    public int currentRound = 0;
    private Coroutine _advanceRoutine;

    private void Awake()
    {
        playerScoreManager = GetComponent<PlayerScoreManager>();
    }

    private void OnEnable()
    {
        GameEvents.OnScoreStateEntered += HandleScoreState;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreStateEntered -= HandleScoreState;
    }

    private void HandleScoreState()
    {
        currentRound++;
        bool isLastRound = currentRound >= maxRounds;
        EndScoreText.SetActive(false);

        ShowLeaderBanner();

        if (currentRound == maxRounds - 1)
        {
            if (GlobalEventBannerUI.Instance != null)
                GlobalEventBannerUI.Instance.ShowBanner("FINAL ROUND", Color.yellow);
        }

        if (questionnairePanel != null)
            questionnairePanel.SetActive(false);

        if (isLastRound)
        {
            EndScoreText.SetActive(true);

            StartCoroutine(ShowQuestionnaireAfterDelay());
            return;
        }

        if (_advanceRoutine != null)
            StopCoroutine(_advanceRoutine);

        _advanceRoutine = StartCoroutine(AdvanceAfterDelay());
    }

    private IEnumerator AdvanceAfterDelay()
    {
        // 1. Warte die normale Anzeigezeit des Scoreboards ab
        yield return new WaitForSeconds(scoreDisplayTime);

        // --- START DER TRANSITION ---
        Image transitionImage = transitionAnimator.GetComponent<Image>();

        // 2. Transition-Image aktivieren und Animation starten
        transitionImage.enabled = true;
        transitionAnimator.SetTrigger("Play");
        OnScoreboardTransitionStarted?.Invoke();
        
        // 3. Warten, bis der Bildschirm verdeckt ist (deine 1.1 Sekunden)
        yield return new WaitForSeconds(1.1f);

        // --- LOGIK IM HINTERGRUND (Bildschirm ist verdeckt) ---

        // 4. Scoreboard UI ausblenden
        if (playerScoreManager != null && playerScoreManager.scoreboardUI != null)
            playerScoreManager.scoreboardUI.SetActive(false);

        // 5. Spiel-Logik für die nächste Runde vorbereiten
        if (currentRound < maxRounds)
        {
            if (playerManagerFinal != null)
                playerManagerFinal.ResetEliminations();

            // State wechseln
            GameEvents.ChangeState(GameState.SurpriseBoxState);
        }

        // 6. Ein winziger Moment warten, damit der neue State geladen ist
        yield return new WaitForSeconds(0.45f);

        // 7. Transition-Image wieder deaktivieren
        transitionImage.enabled = false;
        // --- ENDE DER TRANSITION ---
    }

    private IEnumerator EnableMenuButtonAfterDelay()
    {
        yield return new WaitForSeconds(6f);
        playerScoreManager.SetMenuButtonActiveOrDeactive(true);
        PlayerManager.Instance.HardResetFinalScore();
    }

    private IEnumerator ShowQuestionnaireAfterDelay()
    {
        // 1. Let players see final scoreboard
        yield return new WaitForSeconds(5f);

        // 2. START TRANSITION (fade to black)
        Image transitionImage = transitionAnimator.GetComponent<Image>();
        transitionImage.enabled = true;

        transitionAnimator.SetTrigger("Play");

        // wait until screen is covered
        yield return new WaitForSeconds(1.1f);

        // 3. SWITCH VIEWS while screen is hidden
        if (scoreboardView != null)
            scoreboardView.SetActive(false);

        if (questionnaireView != null)
            questionnaireView.SetActive(true);

        // 4. small delay so new UI settles
        yield return new WaitForSeconds(0.3f);

        // 5. END TRANSITION (reveal questionnaire)
        transitionImage.enabled = false;

        // 6. enable buttons
        playerScoreManager.SetMenuButtonActiveOrDeactive(true);
    }

    private void ShowLeaderBanner()
    {
        if (GlobalEventBannerUI.Instance == null || playerManagerFinal == null)
            return;

        var ranking = playerManagerFinal.GetRoundRanking();

        if (ranking == null || ranking.Count == 0 || ranking[0] == null)
            return;

        int winnerIndex = ranking[0].playerIndex;

        if (winnerIndex == 0)
        {
            GlobalEventBannerUI.Instance.ShowBanner("CUTESY LEADS", Color.white);
        }
        else if (winnerIndex == 1)
        {
            GlobalEventBannerUI.Instance.ShowBanner("JOKESY LEADS", Color.white);
        }
    }
    public void ResetRounds()
    {
        currentRound = 0;
        if (_advanceRoutine != null)
        {
            StopCoroutine(_advanceRoutine);
            _advanceRoutine = null;
        }

        if (playerScoreManager != null)
            playerScoreManager.SetMenuButtonActiveOrDeactive(false);
    }
}