using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerScoreManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerManager   playerManager;
    [SerializeField] public GameObject      scoreboardUI;
    [SerializeField] private TMP_Text[]      placeTexts;    

    // cumulative scores over all rounds
    private Dictionary<PlayerInput, int> _totalScores = new Dictionary<PlayerInput, int>();

    // per‐place points
    private readonly int[] _roundPoints = new[] { 100, 75, 50, 25 };

    private void OnEnable()
    {
        GameEvents.OnScoreStateEntered += ShowScores;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreStateEntered -= ShowScores;
    }

    private void ShowScores()
    {
        var ranking = playerManager.GetRoundRanking(); // winner first

        // 1) Ensure every player is in the dictionary
        foreach (var p in playerManager.players)
            if (!_totalScores.ContainsKey(p))
                _totalScores[p] = 0;

        // 2) Award round points and update totals
        for (int i = 0; i < placeTexts.Length; i++)
        {
            if (i < ranking.Count)
            {
                var pi = ranking[i];
                int pts = _roundPoints[i];
                _totalScores[pi] += pts;

                placeTexts[i].text =
                    $"{i+1}. {pi.gameObject.name} — {pts} (Total: {_totalScores[pi]})";
            }
            else
            {
                placeTexts[i].text = $"{i+1}. ---";
            }
        }

        // 3) show the UI
        scoreboardUI.SetActive(true);
    }

    // Call this whenever a player scores via a kill
    public void AddKillScore(PlayerInput killer, int killPoints = 10)
    {
        if (!_totalScores.ContainsKey(killer))
            _totalScores[killer] = 0;

        _totalScores[killer] += killPoints;
    }
}
