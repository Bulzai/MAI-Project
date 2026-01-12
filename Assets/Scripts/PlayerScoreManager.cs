using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;
using UnityEngine.UI;

public class PlayerScoreManager : MonoBehaviour
{
    
    public static event Action OnPointsIncrease;
    public static event Action OnPointsIncreaseEnd;
    public static event Action OnScoreStateExitTransition;
    
    [Header("References")]
    public PlayerManager playerManager;
    public GameObject scoreboardUI;
    [SerializeField] private Transform rowsContainer; // has Row_1..Row_4 (each with ScoreboardRowUI)


    [Header("Last Round UI")]
    [SerializeField] private GameObject menuButton; // <-- assign in inspector

    [Header("Transition")]
    [SerializeField] private Animator transitionAnimator;

    private List<ScoreboardRowUI> _rowSlots = new();

    private Dictionary<int, int> _totalScores = new();                // playerIndex -> total score
    private Dictionary<int, ScoreboardRowUI> _rows = new();           // playerIndex -> row

    [SerializeField] private Sprite fallbackAvatar;
    private readonly int[] _roundPoints = new[] { 100, 75, 50, 25 };
    


    private static bool IsDestroyed(Object o) => o == null;

    private void OnEnable()
    {
        GameEvents.OnScoreStateEntered += ShowScores;     // Only one scoreboard entry point now
        // GameEvents.OnFinalScoreStateEntered -= removed
    }

    private void OnDisable()
    {
        GameEvents.OnScoreStateEntered -= ShowScores;
    }

    public void ClearCaches()
    {
        StopAllCoroutines();
        _totalScores.Clear();
        _rows.Clear();
        if (scoreboardUI) scoreboardUI.SetActive(false);
    }

    private void CacheRowSlots()
    {
        if (_rowSlots.Count > 0) return;
        if (rowsContainer == null)
        {
            Debug.LogError("[PlayerScoreManager] RowsContainer not set.");
            return;
        }
        _rowSlots = rowsContainer.GetComponentsInChildren<ScoreboardRowUI>(true).ToList();
        if (_rowSlots.Count == 0)
            Debug.LogError("No ScoreboardRowUI children found under RowsContainer.");
    }

    private List<PlayerInput> GetLivePlayers()
    {
        if (playerManager == null) return new List<PlayerInput>();
        playerManager.PruneDestroyedPlayers();
        return playerManager.players.Where(p => !IsDestroyed(p)).OrderBy(p => p.playerIndex).ToList();
    }

    private void EnsureRowsMapped()
    {
        CacheRowSlots();

        var livePlayers = GetLivePlayers();
        int playerCount = Mathf.Clamp(livePlayers.Count, 0, _rowSlots.Count);

        foreach (var r in _rowSlots) r.gameObject.SetActive(false);
        _rows.Clear();

        for (int i = 0; i < playerCount; i++)
        {
            var p = livePlayers[i];
            int idx = p.playerIndex;
            var row = _rowSlots[i];

            row.gameObject.SetActive(true);

            if (!_totalScores.ContainsKey(idx))
                _totalScores[idx] = 0;

            row.SetStatic(p, GetAvatarSprite(idx));
            _rows[idx] = row;
        }
    }

    private void RefreshAllAvatars()
    {
        foreach (var kv in _rows)
            kv.Value.SetAvatar(GetAvatarSprite(kv.Key));
    }

    private Sprite GetAvatarSprite(int playerIndex)
    {
        if (playerManager != null &&
            playerManager.playerAvatars != null &&
            playerIndex >= 0 &&
            playerIndex < playerManager.playerAvatars.Length &&
            playerManager.playerAvatars[playerIndex] != null)
        {
            return playerManager.playerAvatars[playerIndex];
        }
        return fallbackAvatar;
    }

    // -------------------- SCORE FLOW  --------------------

    // Keep signature for event hook.
    private void ShowScores()
    {
        // Default: not last round. RoundController will call SetIsLastRound(true) before entering ScoreState on last round.
        InternalShowScores(isLastRound: false);
    }

    // Call this from RoundController BEFORE you change into ScoreState (for last round).
    public void SetMenuButtonActiveOrDeactive(bool isLastRound)
    {

        menuButton.SetActive(isLastRound);
    }


    private void InternalShowScores(bool isLastRound)
    {
        if (playerManager == null || scoreboardUI == null)
            return;

        // Wir starten die gesamte Sequenz als Coroutine
        StopAllCoroutines();
        StartCoroutine(ScoreboardSequenceCoroutine());
    }

    private IEnumerator ScoreboardSequenceCoroutine()
    {
        // 1. Transition vorbereiten & starten
        Image transitionImage = transitionAnimator.GetComponent<Image>();
        transitionImage.enabled = true;
        transitionAnimator.SetTrigger("Play");
        // 2. Warten, bis der Bildschirm verdeckt ist (deine 1.1 Sekunden)
        yield return new WaitForSeconds(0.8f);

        // 3. UI im Hintergrund vorbereiten (während die Transition noch alles verdeckt)
        EnsureRowsMapped();
        RefreshAllAvatars();

        var shownIndices = _rows.Keys.ToList();
        var oldTotals = new Dictionary<int, int>();
        foreach (var idx in shownIndices)
            oldTotals[idx] = _totalScores.GetValueOrDefault(idx, 0);

        // Punkte berechnen
        var rankingPis = playerManager.GetRoundRanking();
        int place = 0;
        for (int i = 0; i < rankingPis.Count && place < shownIndices.Count; i++)
        {
            var pi = rankingPis[i];
            if (IsDestroyed(pi)) continue;
            int idx = pi.playerIndex;
            if (!_rows.ContainsKey(idx)) continue;

            int pts = place < _roundPoints.Length ? _roundPoints[place] : 0;
            _totalScores[idx] = oldTotals.GetValueOrDefault(idx, 0) + pts;
            place++;
        }

        ApplyOrderByTotalAndSetPlaces();

        int maxTotal = Mathf.Max(
            1,
            _totalScores.Where(k => _rows.ContainsKey(k.Key)).Select(k => k.Value).DefaultIfEmpty(0).Max()
        );

        // 4. Scoreboard jetzt sichtbar machen
        scoreboardUI.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(AnimateAllRows(oldTotals, maxTotal));

        yield return new WaitForSeconds(0.2f);

        // 6. Transition-Image deaktivieren (damit man das Scoreboard sieht)
        transitionImage.enabled = false;

        // 7. Die Balken-Animation der Reihen starten
    }

    private void ApplyOrderByTotalAndSetPlaces()
    {
        var sorted = _totalScores
            .Where(k => _rows.ContainsKey(k.Key))
            .OrderByDescending(k => k.Value)
            .ThenBy(k => k.Key)
            .Select(k => k.Key)
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            int idx = sorted[i];
            var row = _rows[idx];

            row.transform.SetSiblingIndex(i);
            row.SetPlace(i + 1);

            row.SetCrownActive( i == 0);

        }
    }

    private IEnumerator AnimateAllRows(Dictionary<int, int> oldTotals, int maxTotal)
    {
        // Stagger only visible rows, in current UI order
        var ordered = _rows
            .OrderBy(kv => kv.Value.transform.GetSiblingIndex())
            .Select(kv => kv.Key)
            .ToList();

        OnPointsIncrease?.Invoke();

        foreach (var idx in ordered)
        {
            if (!_rows.TryGetValue(idx, out var row)) continue;

            int before = oldTotals.GetValueOrDefault(idx, 0);
            int after = _totalScores.GetValueOrDefault(idx, before);

            StartCoroutine(row.AnimateScores(before, after - before, after, maxTotal, 1.15f));
            yield return new WaitForSeconds(0.07f);
        }
        yield return new WaitForSeconds(1.15f);
        OnPointsIncreaseEnd?.Invoke();
    }

}
