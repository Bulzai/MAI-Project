using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSurvivalTracker : MonoBehaviour
{
    public static PlayerSurvivalTracker Instance { get; private set; }

    private Dictionary<string, float[]> playerSurvivalTimes = new Dictionary<string, float[]>();

    private RoundController roundManager;
    private string sessionID;
    private bool isTrackingTime = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        sessionID = TestingManager.Instance.GetSessionID();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTrackingTime || roundManager == null) return;

        int currentRoundIndex = roundManager.currentRound;

        if (currentRoundIndex >= 0 && currentRoundIndex < roundManager.getMaxRounds())
        {
            var ranking = PlayerManager.Instance.GetRoundRanking();

            foreach (PlayerInput pi in PlayerManager.Instance.players)
            {
                if (pi == null || GameState.MainGameState != GameEvents.CurrentState) continue;

                string playerName = pi.gameObject.name;

                if (!ranking.Contains(pi) && playerSurvivalTimes.ContainsKey(playerName))
                {
                    playerSurvivalTimes[playerName][currentRoundIndex] += Time.deltaTime;
                }
            }
        }
    }

    private void OnEnable()
    {
        roundManager = Object.FindAnyObjectByType<RoundController>();
        if (roundManager == null)
        {
            Debug.LogError("PlayerSurvivalTracker: RoundController not found in scene!");
        }

        GameEvents.OnMainGameStateEntered += InitializeSurvivalTracking;
        GameEvents.OnPlayerEliminated += HandleSurvivalEnd;
        GameEvents.OnMainGameStateExited += StopTrackingOnMatchEnd;

        GameEvents.OnScoreStateEntered += PrintFinalSurvivalTime;
        GameEvents.OnScoreStateEntered += ResetSurvivalTimeList;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= InitializeSurvivalTracking;
        GameEvents.OnPlayerEliminated -= HandleSurvivalEnd;
        GameEvents.OnMainGameStateExited -= StopTrackingOnMatchEnd;

        GameEvents.OnScoreStateEntered -= PrintFinalSurvivalTime;
        GameEvents.OnScoreStateEntered -= ResetSurvivalTimeList;
    }

    private void InitializeSurvivalTracking()
    {
        if (roundManager == null) return;

        isTrackingTime = true;

        // init entries for all players
        foreach (PlayerInput pi in PlayerManager.Instance.players)
        {
            if (pi == null) continue;

            string playerName = pi.gameObject.name;

            if (!playerSurvivalTimes.ContainsKey(playerName)) playerSurvivalTimes.Add(playerName, new float[roundManager.getMaxRounds()]);
        }
    }

    private void HandleSurvivalEnd(PlayerInput eliminatedPlayer)
    {
        if (eliminatedPlayer == null || roundManager == null) return;

        string playerName = eliminatedPlayer.gameObject.name;
        int round = roundManager.currentRound;

        if (playerSurvivalTimes.ContainsKey(playerName) && round >= 0 && round < roundManager.getMaxRounds())
        {
            float finalTime = playerSurvivalTimes[playerName][round];
            string time = finalTime.ToString("F2", CultureInfo.InvariantCulture);

            string data = $"{sessionID},{round+1},SurvivalTime,{playerName},{time}";
            TestingLogger.LogToCSV(data);

            Debug.Log($"{playerName} died in round {round + 1} after surviving {finalTime:F2} seconds.");
        }
    }

    private void StopTrackingOnMatchEnd()
    {
        isTrackingTime = false;
    }

    private void PrintFinalSurvivalTime()
    {
        if (roundManager == null) return;

        // check if the round controller has reached max rounds.
        if (roundManager.currentRound >= roundManager.getMaxRounds())
        {
            Debug.Log("GAME OVER: FINAL MATCH SURVIVAL TIMES");

            foreach (var entry in playerSurvivalTimes)
            {
                string playerName = entry.Key;
                float[] roundTimes = entry.Value;

                string roundBreakdownText = "";

                for (int i = 0; i < roundTimes.Length; i++)
                {
                    roundBreakdownText += $"Round {i + 1}: {roundTimes[i]:F2}s | ";
                }

                Debug.Log($"{playerName} -> {roundBreakdownText}");
            }

        }
    }

    private void ResetSurvivalTimeList()
    {
        if (roundManager.currentRound >= roundManager.getMaxRounds())
            playerSurvivalTimes.Clear();
    }
}
