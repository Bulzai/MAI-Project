using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRangeCollider : MonoBehaviour, IAdaptiveItemScoreCalculator
{
    // max lethality score is 100 from spike
    [SerializeField] int lethalityScore = 50;
    
    [SerializeField] private Collider2D attackRangeCollider;
    private List<Vector3Int> hitCells = new List<Vector3Int>();

    [SerializeField] private float maxHitCellsPossible = 10f;
    private float maxAllowedLethalityScore = 100f;

    private void Update()
    {
    }


    public int GetHowManyCellsWouldBeAboveMaxLethality()
    {
        int counter = 0;
        foreach(Vector3Int cell in hitCells)
        {
            if(HeatMap.Instance.heatmap[cell].cellLethalityScore + lethalityScore > maxAllowedLethalityScore)
                counter++;
        }
        //Debug.Log("Cells above max lethality threshold: " + counter);
        return counter;
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
        if (hitCells.Count == 0) return 0f;
        return ((float)GetTotalLethalityScore() / (hitCells.Count * maxAllowedLethalityScore));
    }

    // 0th entry of totalvisits is current round, 1st entry is previous round, 2nd entry is 2 rounds ago
    //round starts with 0, in round 1 is where first items get placed
    public int[] GetTotalCellVisits()
    {
        int[] totalVisits = {0,0,0};
        int currentRoundIndex = PlayTestDataManager.Instance.roundIndex.Value;
        foreach (Vector3Int cell in hitCells)
        {
            totalVisits[0] += HeatMap.Instance.heatmap[cell].cellVisits[currentRoundIndex-1]
                .Value;
            if (currentRoundIndex > 1)
            {
                totalVisits[1] += HeatMap.Instance.heatmap[cell].cellVisits[currentRoundIndex-2]
                    .Value;
            }

            if (currentRoundIndex > 2)
            {
                totalVisits[2] += HeatMap.Instance.heatmap[cell].cellVisits[currentRoundIndex-3]
                    .Value;
            }
            
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

    public int GetOwnLethalityScore()
    {
        return lethalityScore;
    }

    public float GetNormalizedAttackRangeUtilizationScore()
    {
        return (float)hitCells.Count/maxHitCellsPossible;
    }

    public void Reset()
    {
        hitCells.Clear();
    }
}
