using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuadZoneTracker : MonoBehaviour
{
    [SerializeField] private Collider2D quadUL;
    [SerializeField] private Collider2D quadUR;
    [SerializeField] private Collider2D quadLR;
    [SerializeField] private Collider2D quadLL;

    // one round of quad time data
    [System.Serializable]
    public class OneRoundQuadTime
    {
        public float ULTime = 0f;
        public float URTime = 0f;
        public float LRTime = 0f;
        public float LLTime = 0f;
    }

    private Dictionary<string, List<OneRoundQuadTime>> playerQuadTimes = new Dictionary<string, List<OneRoundQuadTime>>();

    private bool isTracking = false;
    private RoundController roundManager;

    private void OnEnable()
    {
        roundManager = Object.FindAnyObjectByType<RoundController>();
        if (roundManager == null)
        {
            Debug.LogError("PlayerDeathTracker: RoundController not found in scene!");
        }

        GameEvents.OnMainGameStateEntered += InitializeQuadTimeTracking;
        GameEvents.OnMainGameStateExited += StopTrackingQuadTime;

        GameEvents.OnScoreStateEntered += PrintFinalQuadTimes;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= InitializeQuadTimeTracking;
        GameEvents.OnMainGameStateExited -= StopTrackingQuadTime;

        GameEvents.OnScoreStateEntered -= PrintFinalQuadTimes;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isTracking) return;

        foreach (PlayerInput pi in PlayerManager.Instance.players)
        {
            if (pi == null || !pi.gameObject.activeSelf) continue;

            Collider2D playerCollider = pi.GetComponentInChildren<Collider2D>();
            if (playerCollider == null) continue;

            string player = pi.gameObject.name;

            if (playerQuadTimes.ContainsKey(player))
            {
                List<OneRoundQuadTime> history = playerQuadTimes[player];
                OneRoundQuadTime currentRoundData = history[history.Count - 1];

                if (quadUL != null && playerCollider.IsTouching(quadUL)) currentRoundData.ULTime += Time.deltaTime;
                else if (quadUR != null && playerCollider.IsTouching(quadUR)) currentRoundData.URTime += Time.deltaTime;
                else if (quadLR != null && playerCollider.IsTouching(quadLR)) currentRoundData.LRTime += Time.deltaTime;
                else if (quadLL != null && playerCollider.IsTouching(quadLL)) currentRoundData.LLTime += Time.deltaTime;
            }
        }
    }

    private void InitializeQuadTimeTracking()
    {
        foreach (PlayerInput pi in PlayerManager.Instance.players)
        {
            if (pi == null) continue;
            string player = pi.gameObject.name;

            if (!playerQuadTimes.ContainsKey(player)) playerQuadTimes[player] = new List<OneRoundQuadTime>();

            playerQuadTimes[player].Add(new OneRoundQuadTime());
            isTracking = true;
        }
    }

    private void StopTrackingQuadTime()
    {
        isTracking = false;
    }

    private void PrintFinalQuadTimes()
    {
        if (roundManager == null) return;

        if (roundManager.currentRound >= roundManager.getMaxRounds())
        {
            Debug.Log("GAME OVER: FINAL QUAD TIMES");

            foreach (var entry in playerQuadTimes)
            {
                string playerName = entry.Key;
                List<OneRoundQuadTime> roundTimes = entry.Value;

                string roundBreakdownText = "";

                for (int i = 0; i < roundTimes.Count; i++)
                {
                    roundBreakdownText += 
                        $"Round {i + 1} [UL: {roundTimes[i].ULTime:F1}s, " +
                        $"UR: {roundTimes[i].URTime:F1}s, " +
                        $"LR: {roundTimes[i].LRTime:F1}s, " +
                        $"LL: {roundTimes[i].LLTime:F1}s] | ";
                }

                Debug.Log($"{playerName} Quad Times -> {roundBreakdownText}");
            }
        }
    }
}
