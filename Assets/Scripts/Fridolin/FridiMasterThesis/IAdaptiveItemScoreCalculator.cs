using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAdaptiveItemScoreCalculator
{
    public int GetTotalLethalityScore();
    public float GetAverageLethalityScore();
    public int GetTotalCellVisits();
    public int GetMaxLethalityScore();
    public void ApplyLethality();
    public void UpdateHitCells();
    public void Reset();
}
