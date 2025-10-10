using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerScoreManager : MonoBehaviour
{
    [Header("References")]
    public PlayerManager playerManager;
    public GameObject scoreboardUI;
    [SerializeField] private Transform rowsContainer; // has Row_1..Row_4 (each with ScoreboardRowUI)

    private Dictionary<PlayerInput, int> _totalScores = new();
    private Dictionary<PlayerInput, ScoreboardRowUI> _rows = new();
    private List<ScoreboardRowUI> _rowSlots = new();


    [SerializeField] private Sprite fallbackAvatar; 

    private readonly int[] _roundPoints = new[] { 100, 75, 50, 25 };

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

    private void CacheRowSlots()
    {
        if (_rowSlots.Count > 0) return;
        _rowSlots = rowsContainer.GetComponentsInChildren<ScoreboardRowUI>(true).ToList();
        if (_rowSlots.Count == 0) Debug.LogError("No ScoreboardRowUI children found under RowsContainer.");
    }

    // Enable only first N rows (N = player count 2..4) and map them to players
    private void EnsureRowsMapped()
    {
        CacheRowSlots();

        int playerCount = playerManager.playerCount;

        // Hide all rows first
        foreach (var r in _rowSlots) r.gameObject.SetActive(false);

        // Clear previous mapping
        _rows.Clear();

        // Show first N rows and map to players in order
        for (int i = 0; i < playerCount; i++)
        {
            var p = playerManager.players[i];
            var row = _rowSlots[i];
            row.gameObject.SetActive(true);

            if (!_totalScores.ContainsKey(p)) _totalScores[p] = 0;

            var sprite = GetAvatarSprite(p);
            row.SetStatic(p, sprite);

            _rows[p] = row;
        }
    }
    private void RefreshAllAvatars()
    {
        foreach (var kv in _rows)
            kv.Value.SetAvatar(GetAvatarSprite(kv.Key));
    }

    private Sprite GetAvatarSprite(PlayerInput pi)
    {
        // 1) Exact child "Sprite" on the player
        var spriteTf = pi.transform.Find("Sprite");
        if (spriteTf)
        {
            var sr = spriteTf.GetComponent<SpriteRenderer>();
            if (sr && sr.sprite) return sr.sprite;
        }

        // 2) Any child named "Sprite" with a SpriteRenderer
        var named = pi.GetComponentsInChildren<SpriteRenderer>(true)
                      .FirstOrDefault(r => r.gameObject.name == "Sprite" && r.sprite != null);
        if (named) return named.sprite;

        // 3) Fallback: first non-null SpriteRenderer (avoids grabbing "Weapon"/"TrailRenderer")
        var first = pi.GetComponentsInChildren<SpriteRenderer>(true)
                      .FirstOrDefault(r => r.enabled && r.sprite != null);
        return first ? first.sprite : null;
    }


    private void ShowScores()
    {

        RefreshAllAvatars();


        EnsureRowsMapped();


        ReorderRowsByCurrentRanking();

        var shownPlayers = _rows.Keys.ToList();

        var ranking = playerManager.GetRoundRanking(); // winner first

        // Snapshot old totals only for shown players
        var oldTotals = new Dictionary<PlayerInput, int>();
        foreach (var p in shownPlayers)
            oldTotals[p] = _totalScores.GetValueOrDefault(p, 0);

        // Award placement points, but only to shown players
        int place = 0;
        for (int i = 0; i < ranking.Count && place < shownPlayers.Count; i++)
        {
            var pi = ranking[i];
            if (!_rows.ContainsKey(pi)) continue; // skip players that aren't shown
            int pts = place < _roundPoints.Length ? _roundPoints[place] : 0;
            _totalScores[pi] = oldTotals[pi] + pts;
            place++;
        }

        // Normalize only across shown players
        int maxTotal = Mathf.Max(1, _totalScores
            .Where(k => _rows.ContainsKey(k.Key))
            .Select(k => k.Value).DefaultIfEmpty(0).Max());

        StopAllCoroutines();
        StartCoroutine(AnimateAllRows(oldTotals, maxTotal));

        scoreboardUI.SetActive(true);
    }

    private IEnumerator AnimateAllRows(Dictionary<PlayerInput, int> oldTotals, int maxTotal)
    {
        float stagger = 0.07f;
        foreach (var p in playerManager.players)
        {
            if (!_rows.ContainsKey(p)) continue; // only shown rows
            var row = _rows[p];
            int before = oldTotals.GetValueOrDefault(p, 0);
            int after = _totalScores[p];
            StartCoroutine(row.AnimateScores(before, after - before, after, maxTotal, 1.15f));
            yield return new WaitForSeconds(stagger);
        }
    }

    private void ShowFinalScores()
    {
        RefreshAllAvatars();

        EnsureRowsMapped();


        ReorderRowsByCurrentRanking();

        // Sort only shown players
        var sorted = _totalScores
            .Where(k => _rows.ContainsKey(k.Key))
            .OrderByDescending(k => k.Value)
            .Select(k => k.Key)
            .ToList();

        // Reorder visible rows
        for (int i = 0; i < sorted.Count; i++)
            _rows[sorted[i]].transform.SetSiblingIndex(i);

        int maxTotal = Mathf.Max(1, _totalScores
            .Where(k => _rows.ContainsKey(k.Key))
            .Select(k => k.Value).DefaultIfEmpty(0).Max());

        StopAllCoroutines();
        StartCoroutine(FinalSnap(maxTotal));

        scoreboardUI.SetActive(true);
    }

    private IEnumerator FinalSnap(int maxTotal)
    {
        foreach (var kv in _rows)
        {
            int total = _totalScores[kv.Key];
            StartCoroutine(kv.Value.AnimateScores(total, 0, total, maxTotal, 0.35f));
        }
        yield return null;
    }

    private void ReorderRowsByCurrentRanking()
    {
        // winner first from your PlayerManager
        var ranking = playerManager.GetRoundRanking();

        int uiIndex = 0;
        foreach (var pi in ranking)
        {
            if (_rows.TryGetValue(pi, out var row))
            {
                row.transform.SetSiblingIndex(uiIndex);
                row.SetPlace(uiIndex + 1); // 1., 2., 3., ..
                uiIndex++;
            }
        }
    }


}
