using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAdaptiveItemScoreCalculator
{
    public int GetTotalLethalityScore();
    public float GetNormalizedAverageLethalityScore();
    public int GetNormalizedTotalCellVisits();
    public int GetMaxLethalityScore();
    public void ApplyLethality();
    public void UpdateHitCells();
    public void Reset();
    public float GetNormalizedAttackRangeUtilizationScore();
}
