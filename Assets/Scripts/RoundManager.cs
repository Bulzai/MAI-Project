using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RoundController : MonoBehaviour
{
    [Header("Round Settings")]
    [SerializeField] private int maxRounds = 3;
    [SerializeField] private float scoreDisplayTime = 5f;
    public PlayerManager playerManagerFinal;
    [SerializeField] PlayerScoreManager playerScoreManager;


    private int currentRound = 0;

    private void OnEnable()
    {
        // Whenever we finish a round…
        GameEvents.OnScoreStateEntered += HandleScoreState;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreStateEntered -= HandleScoreState;
    }

    private void HandleScoreState()
    {
        currentRound++;
        StartCoroutine(AdvanceAfterDelay());
    }

    private IEnumerator AdvanceAfterDelay()
    {
        // 1) Let the scoreboard stay up for a bit
        yield return new WaitForSeconds(scoreDisplayTime);

        // 2) Hide the scoreboard UI
        var scoreMgr = FindObjectOfType<PlayerScoreManager>();
        if (scoreMgr != null)
            scoreMgr.scoreboardUI.SetActive(false);
        playerScoreManager.scoreboardUI.SetActive(false);

        // 3) Decide where to go next
        if (currentRound < maxRounds)
        {
            if(playerManagerFinal != null)
                playerManagerFinal.ResetEliminations();


            Debug.Log("Here in RoundController");
            // next round: go back to item‐picking
            GameEvents.ChangeState(GameState.SurpriseBoxState);
        }
        else
        {
            // all rounds done: show final results
            // we can reuse ScoreState or make a dedicated FinalScoreState
            GameEvents.ChangeState(GameState.FinalScoreState);

            currentRound = 0; 
        }
    }
}
