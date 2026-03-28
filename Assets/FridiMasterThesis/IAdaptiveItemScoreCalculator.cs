using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAdaptiveItemScoreCalculator
{
    public int GetTotalLethalityScore();
    public float GetNormalizedAverageLethalityScore();
    
    
    // 0th entry of totalvisits is current round, 1st entry is previous round, 2nd entry is 2 rounds ago
    //round starts with 0, in round 1 is where first items get placed
    public int[] GetTotalCellVisits();
    public int GetMaxLethalityScore();
    public int GetOwnLethalityScore();
    public void ApplyLethality();
    public void UpdateHitCells();
    public void Reset();
    public float GetNormalizedAttackRangeUtilizationScore();
    public int GetHowManyCellsWouldBeAboveMaxLethality();

}
