using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSurvivalTracker : MonoBehaviour
{
    public static PlayerSurvivalTracker Instance { get; private set; }

    private Dictionary<string, float[]> playerSurvivalTimes = new Dictionary<string, float[]>();

    private RoundController roundManager;
    private bool isTrackingTime = false;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTrackingTime || roundManager == null) return;

        int currentRoundIndex = roundManager.currentRound;

        if (currentRoundIndex >= 0 && currentRoundIndex < roundManager.getMaxRounds()) 
        {
            var ranking = PlayerManager.Instance.GetRoundRanking();

            foreach(PlayerInput pi in PlayerManager.Instance.players)
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

        GameEvents.OnMainGameStateEntered += StartSurvivalTracking;
        GameEvents.OnPlayerEliminated += HandleSurvivalEnd;
        GameEvents.OnMainGameStateExited += StopTrackingOnMatchEnd;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= StartSurvivalTracking;
        GameEvents.OnPlayerEliminated -= HandleSurvivalEnd;
        GameEvents.OnMainGameStateExited -= StopTrackingOnMatchEnd;
    }

    private void StartSurvivalTracking()
    {
        if (roundManager == null) return;

        isTrackingTime = true;
        int currentRoundIndex = roundManager.currentRound;

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
        int currentRoundIndex = roundManager.currentRound;

        if (playerSurvivalTimes.ContainsKey(playerName) && currentRoundIndex >= 0 && currentRoundIndex < roundManager.getMaxRounds())
        {
            float finalTime = playerSurvivalTimes[playerName][currentRoundIndex];
            Debug.Log($"{playerName} died in round {currentRoundIndex + 1} after surviving {finalTime:F2} seconds.");
        }
    }

    private void StopTrackingOnMatchEnd()
    {
        isTrackingTime=false;
    }
}
