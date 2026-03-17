using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRangeCollider : MonoBehaviour, IAdaptiveItemScoreCalculator
{
    // max lethality score is 100 from spike
    [SerializeField] int lethalityScore = 50;
    
    [SerializeField] private Collider2D attackRangeCollider;
    private List<Vector3Int> hitCells = new List<Vector3Int>();

    [SerializeField] private int maxHitCellsPossible = 10;

    private void Update()
    {
        UpdateHitCells();
    }
    
    public void UpdateHitCells()
    {
        hitCells.Clear();
        
        if (attackRangeCollider == null)
        {
            Debug.LogWarning($"AttackRangeCOllider No collider found on {name}.");
            return;
        }

        var grid = GridPlacementSystem.Instance.gridLayout;
        Bounds bounds = attackRangeCollider.bounds;
        float step = grid.cellSize.x * 0.25f; // smaller step = more reliable coverage

        // Expand slightly to avoid missing border points
        bounds.Expand(0.01f);

        for (float x = bounds.min.x; x <= bounds.max.x; x += step)
        {
            for (float y = bounds.min.y; y <= bounds.max.y; y += step)
            {
                Vector2 p = new Vector2(x, y);
                if (attackRangeCollider.OverlapPoint(p))
                {
                    Vector3Int cell = grid.WorldToCell(p);
                    // Only add if in heatmap!
                    if (HeatMap.Instance.heatmap.ContainsKey(cell) && !hitCells.Contains(cell) && GridPlacementSystem.Instance.CanTakeCell(cell))
                    {
                        hitCells.Add(cell);
                    }
                }
            }
        }
        Debug.Log("UpdatehitCells count for " + name + ": " + hitCells.Count);

    }

    public int GetTotalLethalityScore()
    {
        int totalScore = 0;
        foreach(Vector3Int cell in hitCells)
        {
            totalScore += HeatMap.Instance.heatmap[cell].cellLethalityScore;
        }
        return totalScore;
    }

    public float GetNormalizedAverageLethalityScore()
    {
        Debug.Log("GetNormalizedAverageLethalityScore for " + name + ": " + ((float)GetTotalLethalityScore() / hitCells.Count) / 100f);
        if (hitCells.Count == 0) return 0f;
        return ((float)GetTotalLethalityScore() / hitCells.Count) / 100f;
    }

    public int GetNormalizedTotalCellVisits()
    {
        int totalVisits = 0;
        foreach (Vector3Int cell in hitCells)
        {
            totalVisits += HeatMap.Instance.heatmap[cell].cellVisits[PlayTestDataManager.Instance.roundIndex.Value]
                .Value;
        }

        return totalVisits;
    }

    public int GetMaxLethalityScore()
    {
        int maxScore = 0;
        foreach(Vector3Int cell in hitCells)
        {
            if(HeatMap.Instance.heatmap[cell].cellLethalityScore > maxScore)
                maxScore = HeatMap.Instance.heatmap[cell].cellLethalityScore;
        }
        return maxScore;
    }

    public void ApplyLethality()
    {
        foreach(Vector3Int cell in hitCells)
        {
            HeatMap.Instance.heatmap[cell].cellLethalityScore += lethalityScore;
        }
    }

    public float GetNormalizedAttackRangeUtilizationScore()
    {
        Debug.Log("GetNormalizedAttackRangeUtilizationScore for " + name + ": " + (float)hitCells.Count/maxHitCellsPossible);
        return hitCells.Count/maxHitCellsPossible;
    }

    public void Reset()
    {
        hitCells.Clear();
    }
}
