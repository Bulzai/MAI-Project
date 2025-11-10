using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerScoreManager : MonoBehaviour
{
    [Header("References")]
    public PlayerManager playerManager;
    public GameObject scoreboardUI;
    [SerializeField] private Transform rowsContainer; // has Row_1..Row_4 (each with ScoreboardRowUI)

    // Cache UI row slots found under rowsContainer
    private List<ScoreboardRowUI> _rowSlots = new();

    // >>> CHANGED: Key everything by playerIndex (int) to avoid destroyed PlayerInput refs
    private Dictionary<int, int> _totalScores = new();                // playerIndex -> total score
    private Dictionary<int, ScoreboardRowUI> _rows = new();           // playerIndex -> row

    [SerializeField] private Sprite fallbackAvatar;

    private readonly int[] _roundPoints = new[] { 100, 75, 50, 25 };

    private static bool IsDestroyed(Object o) => o == null;

    private void OnEnable()
    {
        GameEvents.OnScoreStateEntered += ShowScores;
        GameEvents.OnFinalScoreStateEntered += ShowFinalScores;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreStateEntered -= ShowScores;
        GameEvents.OnFinalScoreStateEntered -= ShowFinalScores;
    }

    /// <summary>Call this from your ResetGame flow to nuke stale mappings.</summary>
    public void ClearCaches()
    {
        StopAllCoroutines();
        _totalScores.Clear();
        _rows.Clear();
        // Don't clear _rowSlots; it's just the prefab references under the container
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

    /// <summary>Current valid players (skip destroyed) in index order.</summary>
    private List<PlayerInput> GetLivePlayers()
    {
        if (playerManager == null) return new List<PlayerInput>();
        // prune destroyed refs if any linger
        playerManager.PruneDestroyedPlayers();
        return playerManager.players.Where(p => !IsDestroyed(p)).OrderBy(p => p.playerIndex).ToList();
    }

    /// <summary>
    /// Enable only first N rows (N = live player count 2..4) and map them to players by index.
    /// Populates _rows (playerIndex -> row), and ensures _totalScores has an entry per mapped player.
    /// Also calls row.SetStatic(...) once with the live PlayerInput and avatar.
    /// </summary>
    private void EnsureRowsMapped()
    {
        CacheRowSlots();

        var livePlayers = GetLivePlayers();
        int playerCount = Mathf.Clamp(livePlayers.Count, 0, _rowSlots.Count);

        // Hide all rows first
        foreach (var r in _rowSlots) r.gameObject.SetActive(false);

        _rows.Clear();

        for (int i = 0; i < playerCount; i++)
        {
            var p = livePlayers[i];
            var idx = p.playerIndex;
            var row = _rowSlots[i];

            row.gameObject.SetActive(true);

            if (!_totalScores.ContainsKey(idx))
                _totalScores[idx] = 0;

            // Assign avatar once (live player ref is OK here)
            var avatar = GetAvatarSprite(idx);
            row.SetStatic(p, avatar);

            _rows[idx] = row;
        }
    }

    /// <summary>Refresh just the avatars on already mapped rows (by index).</summary>
    private void RefreshAllAvatars()
    {
        foreach (var kv in _rows)
        {
            int idx = kv.Key;
            kv.Value.SetAvatar(GetAvatarSprite(idx));
        }
    }

    /// <summary>Get avatar by playerIndex from PlayerManager arrays (stable across destroys).</summary>
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

    // -------------------- SCORE FLOWS --------------------

    private void ShowScores()
    {
        if (playerManager == null || scoreboardUI == null)
            return;

        EnsureRowsMapped();     // builds _rows by index for current roster
        RefreshAllAvatars();    // uses index-based avatars
        ReorderRowsByCurrentRanking();

        var shownIndices = _rows.Keys.ToList(); // only rows that are visible

        // Snapshot old totals only for shown players (by index)
        var oldTotals = new Dictionary<int, int>();
        foreach (var idx in shownIndices)
            oldTotals[idx] = _totalScores.GetValueOrDefault(idx, 0);

        // Award placement points according to current round ranking (winner first)
        var rankingPis = playerManager.GetRoundRanking(); // List<PlayerInput> winner..last
        int place = 0;
        for (int i = 0; i < rankingPis.Count && place < shownIndices.Count; i++)
        {
            var pi = rankingPis[i];
            if (IsDestroyed(pi)) continue;
            int idx = pi.playerIndex;
            if (!_rows.ContainsKey(idx)) continue; // only shown players

            int pts = place < _roundPoints.Length ? _roundPoints[place] : 0;
            _totalScores[idx] = oldTotals.GetValueOrDefault(idx, 0) + pts;
            place++;
        }

        // Normalize only across shown players
        int maxTotal = Mathf.Max(
            1,
            _totalScores
                .Where(k => _rows.ContainsKey(k.Key))
                .Select(k => k.Value)
                .DefaultIfEmpty(0)
                .Max()
        );

        StopAllCoroutines();
        StartCoroutine(AnimateAllRows(oldTotals, maxTotal));

        scoreboardUI.SetActive(true);
    }

    private IEnumerator AnimateAllRows(Dictionary<int, int> oldTotals, int maxTotal)
    {
        // stagger only visible rows, in the current UI order
        float stagger = 0.07f;

        // order by current layout order (sibling index)
        var ordered = _rows
            .OrderBy(kv => kv.Value.transform.GetSiblingIndex())
            .Select(kv => kv.Key) // idx
            .ToList();

        foreach (var idx in ordered)
        {
            if (!_rows.TryGetValue(idx, out var row)) continue;

            int before = oldTotals.GetValueOrDefault(idx, 0);
            int after = _totalScores.GetValueOrDefault(idx, before);

            StartCoroutine(row.AnimateScores(before, after - before, after, maxTotal, 1.15f));
            yield return new WaitForSeconds(0.07f);
        }
    }

    private void ShowFinalScores()
    {
        if (playerManager == null || scoreboardUI == null)
            return;

        EnsureRowsMapped();
        RefreshAllAvatars();
        ReorderRowsByCurrentRanking();

        // Sort only shown players by final total descending
        var sortedByScore = _totalScores
            .Where(k => _rows.ContainsKey(k.Key))
            .OrderByDescending(k => k.Value)
            .Select(k => k.Key) // idx
            .ToList();

        for (int i = 0; i < sortedByScore.Count; i++)
        {
            int idx = sortedByScore[i];
            _rows[idx].transform.SetSiblingIndex(i);
        }

        int maxTotal = Mathf.Max(
            1,
            _totalScores
                .Where(k => _rows.ContainsKey(k.Key))
                .Select(k => k.Value)
                .DefaultIfEmpty(0)
                .Max()
        );

        StopAllCoroutines();
        StartCoroutine(FinalSnap(maxTotal));

        scoreboardUI.SetActive(true);
    }

    private IEnumerator FinalSnap(int maxTotal)
    {
        foreach (var kv in _rows)
        {
            int idx = kv.Key;
            int total = _totalScores.GetValueOrDefault(idx, 0);
            StartCoroutine(kv.Value.AnimateScores(total, 0, total, maxTotal, 0.35f));
        }
        yield return null;
    }

    // -------------------- UI ORDERING --------------------

    private void ReorderRowsByCurrentRanking()
    {
        if (playerManager == null) return;

        var ranking = playerManager.GetRoundRanking(); // List<PlayerInput> winner first

        int uiIndex = 0;
        foreach (var pi in ranking)
        {
            if (IsDestroyed(pi)) continue;
            int idx = pi.playerIndex;

            if (_rows.TryGetValue(idx, out var row))
            {
                row.transform.SetSiblingIndex(uiIndex);
                row.SetPlace(uiIndex + 1); // 1., 2., 3., ..
                uiIndex++;
            }
        }
    }
}
