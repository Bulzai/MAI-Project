using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDeathTrack : MonoBehaviour
{
    public static PlayerDeathTrack Instance { get; private set; }

    private Dictionary<string, string[]> playerDeathRecords = new Dictionary<string, string[]>();

    private RoundController roundManager;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        roundManager = Object.FindAnyObjectByType<RoundController>();
        if (roundManager == null)
        {
            Debug.LogError("PlayerDeathTracker: RoundController not found in scene!");
        }

        GameEvents.OnMainGameStateEntered += InitializeRoundTracking;
        GameEvents.OnPlayerEliminated += RecordPlayerDeath;
        GameEvents.OnScoreStateEntered += PrintFinalPlayerDeathList;
        //GameEvents.OnMenuStateEntered += ResetPlayerDeathList;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= InitializeRoundTracking;
        GameEvents.OnPlayerEliminated -= RecordPlayerDeath;
        GameEvents.OnScoreStateEntered -= PrintFinalPlayerDeathList;
        //GameEvents.OnMenuStateEntered -= ResetPlayerDeathList;
    }

    private void InitializeRoundTracking()
    {
        if (roundManager == null) return;

        foreach (PlayerInput pi in PlayerManager.Instance.players)
        {
            if (pi == null) continue;
            string playerName = pi.gameObject.name;

            if (!playerDeathRecords.ContainsKey(playerName)) playerDeathRecords.Add(playerName, new string[roundManager.getMaxRounds()]);

            // set default to survived
            playerDeathRecords[playerName][roundManager.currentRound] = "Survived";
        }
    }

    private void RecordPlayerDeath(PlayerInput eliminatedPlayer)
    {
        if (eliminatedPlayer == null || roundManager == null) return;

        int currentRoundIndex = roundManager.currentRound;
        string playerName = eliminatedPlayer.gameObject.name;

        var healthSystem = eliminatedPlayer.GetComponentInChildren<PlayerHealthSystem>();

        if (playerDeathRecords.ContainsKey(playerName) && currentRoundIndex >= 0 && currentRoundIndex < roundManager.getMaxRounds())
        {
            string finalCause = "Unknown";

            if (healthSystem != null) finalCause = healthSystem.lastTouched;

            // remove (Clone), Collider
            finalCause = System.Text.RegularExpressions.Regex.Replace(finalCause, @"\(Clone\)", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            finalCause = System.Text.RegularExpressions.Regex.Replace(finalCause, @"Collider", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            finalCause = finalCause.Trim();
            finalCause = finalCause.ToLower();

            playerDeathRecords[playerName][currentRoundIndex] = finalCause;

            Debug.Log($"{playerName} died in round {currentRoundIndex + 1} from {finalCause}");
        }
    }

    private void PrintFinalPlayerDeathList ()
    {
        if (roundManager == null) return;

        // check if the round controller has reached max rounds.
        if (roundManager.currentRound >= roundManager.getMaxRounds())
        {
            Debug.LogWarning("GAME OVER: FINAL MATCH DEATH CAUSES");

            foreach (var entry in playerDeathRecords)
            {
                string playerName = entry.Key;
                string[] roundStatuses = entry.Value;

                string breakdownText = "";

                for (int i = 0; i < roundStatuses.Length; i++)
                {
                    breakdownText += $"Round {i + 1}: {roundStatuses[i]} | ";
                }

                Debug.Log($"{playerName} -> {breakdownText}");
            }

        }
    }

    private void ResetPlayerDeathList()
    {
        StartCoroutine(WaitAndResetList());
    }

    private IEnumerator WaitAndResetList()
    {
        yield return new WaitForSeconds(3);

        if (roundManager.currentRound >= roundManager.getMaxRounds())
            playerDeathRecords.Clear();
    }
}
