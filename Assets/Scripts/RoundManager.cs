using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Wichtig für das Image-Component

public class RoundController : MonoBehaviour
{
    public static event System.Action OnScoreboardTransitionStarted;
    [Header("Round Settings")]
    [SerializeField] private int maxRounds = 4;
    [SerializeField] private float scoreDisplayTime = 5f;

    [Header("Transition")]
    [SerializeField] private Animator transitionAnimator; // Hier den Animator zuweisen

    [Header("Final Questionnaire Flow")]
    [SerializeField] private GameObject scoreboardView;
    [SerializeField] private GameObject questionnairePanel;
    [SerializeField] private float finalScoreDisplayTime = 3.5f;

    [Header("Final Button")]
    [SerializeField] private GameObject goToEndScreenButton;

    public GameObject EndScoreText;
    public PlayerManager playerManagerFinal;
    private PlayerScoreManager playerScoreManager;

    public int currentRound = 0;
    private Coroutine _advanceRoutine;

    private void Awake()
    {
        playerScoreManager = GetComponent<PlayerScoreManager>();

        if (goToEndScreenButton != null)
            goToEndScreenButton.SetActive(false);

        if (questionnairePanel != null)
            questionnairePanel.SetActive(false);

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

        if (goToEndScreenButton != null)
            goToEndScreenButton.SetActive(false);

        if (EndScoreText != null)
            EndScoreText.SetActive(false);

        if (questionnairePanel != null)
            questionnairePanel.SetActive(false);

        if (isLastRound)
        {
            if (EndScoreText != null)
                EndScoreText.SetActive(true);

            if (goToEndScreenButton != null)
                goToEndScreenButton.SetActive(true);

            return;
        }

        if (_advanceRoutine != null)
            StopCoroutine(_advanceRoutine);

        _advanceRoutine = StartCoroutine(AdvanceAfterDelay());
    }

    private IEnumerator AdvanceAfterDelay()
    {
        yield return new WaitForSeconds(scoreDisplayTime);

        Image transitionImage = transitionAnimator.GetComponent<Image>();

        transitionImage.enabled = true;
        transitionAnimator.SetTrigger("Play");
        OnScoreboardTransitionStarted?.Invoke();

        yield return new WaitForSeconds(1.1f);

        if (playerScoreManager != null && playerScoreManager.scoreboardUI != null)
            playerScoreManager.scoreboardUI.SetActive(false);

        if (currentRound < maxRounds)
        {
            if (playerManagerFinal != null)
                playerManagerFinal.ResetEliminations();

            GameEvents.ChangeState(GameState.SurpriseBoxState);
        }

        yield return new WaitForSeconds(0.45f);

        transitionImage.enabled = false;
    }

    private IEnumerator ShowQuestionnaireAfterDelay()
    {
        yield return new WaitForSeconds(finalScoreDisplayTime);

        Image transitionImage = null;

        if (transitionAnimator != null)
        {
            transitionImage = transitionAnimator.GetComponent<Image>();

            if (transitionImage != null)
                transitionImage.enabled = true;

            transitionAnimator.SetTrigger("Play");

            yield return new WaitForSeconds(1.1f);
        }

        if (scoreboardView != null)
            scoreboardView.SetActive(false);

        if (questionnairePanel != null)
            questionnairePanel.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        if (transitionImage != null)
            transitionImage.enabled = false;

        if (playerScoreManager != null)
            playerScoreManager.SetMenuButtonActiveOrDeactive(true);
    }

    public void ResetRounds()
    {
        currentRound = 0;

        if (_advanceRoutine != null)
        {
            StopCoroutine(_advanceRoutine);
            _advanceRoutine = null;
        }

        if (goToEndScreenButton != null)
            goToEndScreenButton.SetActive(false);

        if (questionnairePanel != null)
            questionnairePanel.SetActive(false);

        if (scoreboardView != null)
            scoreboardView.SetActive(false);

        if (playerScoreManager != null)
            playerScoreManager.SetMenuButtonActiveOrDeactive(false);
    }
}