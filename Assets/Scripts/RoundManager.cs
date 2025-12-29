using System.Collections;
using UnityEngine;

public class RoundController : MonoBehaviour
{
    [Header("Round Settings")]
    [SerializeField] private int maxRounds = 3;
    [SerializeField] private float scoreDisplayTime = 5f;

    public PlayerManager playerManagerFinal;
    private PlayerScoreManager playerScoreManager;

    private int currentRound = 0;
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


        // IMPORTANT: If it's the last round, do NOT auto-advance anywhere.
        if (isLastRound)
        {
            StartCoroutine(EnableMenuButtonAfterDelay());
            return;
        }

        if (_advanceRoutine != null)
            StopCoroutine(_advanceRoutine);

        _advanceRoutine = StartCoroutine(AdvanceAfterDelay());
    }
    
    private IEnumerator EnableMenuButtonAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        playerScoreManager.SetMenuButtonActiveOrDeactive(true);
        PlayerManager.Instance.HardResetFinalScore();
    }

    private IEnumerator AdvanceAfterDelay()
    {
        yield return new WaitForSeconds(scoreDisplayTime);

        // Hide scoreboard UI
        if (playerScoreManager != null && playerScoreManager.scoreboardUI != null)
            playerScoreManager.scoreboardUI.SetActive(false);

        if (currentRound < maxRounds)
        {
            if (playerManagerFinal != null)
                playerManagerFinal.ResetEliminations();

            GameEvents.ChangeState(GameState.SurpriseBoxState);
        }
    }

    // Call this when starting a new run / back to main menu, etc.
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
