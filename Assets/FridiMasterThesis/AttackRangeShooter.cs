using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRangeShooter : MonoBehaviour, IAdaptiveItemScoreCalculator
{
    [SerializeField] private Transform[] sideFirePoints;
    [SerializeField] private Transform mainFirePoint;
    [SerializeField] private LayerMask hitLayers = -1;  // ← ADD THIS LINE
    [SerializeField] private Transform direction;
    // max lethality score is 100 from spike
    private int mainRayLethalityScore = 51;
    private int sideRayLethalityScore = 51;
    [SerializeField] private int maxRange = 46;
    private List<Vector3Int> mainCells = new List<Vector3Int>();
    private List<Vector3Int>[] sideCells = new List<Vector3Int>[sideFirePointsLength];
    private static int sideFirePointsLength = 2;
    private float mainRayActualLength;
    private float[] sideRayActualLengths = new float[sideFirePointsLength];
    private float maxAllowedLethalityScore = 100f;
    private float circleCastRadius = 0.5f;
    
    private void Update()
    {
    }
    
    void Awake()
    {
        sideCells = new List<Vector3Int>[sideFirePointsLength];
        for (int i = 0; i < sideFirePointsLength; i++)
        {
            sideCells[i] = new List<Vector3Int>(); // Initialize each list!
        }
    }
    
    public void Shoot()
    {
        // MAIN - Always draw + log
        Vector2 mainOrigin = mainFirePoint.position;
        Vector2 mainDir = direction.right.normalized; 
       
        if (transform.localScale.x < 0f)
        {
            mainDir.y = -mainDir.y;
        }
    
        RaycastHit2D mainHit = Physics2D.CircleCast(mainOrigin, circleCastRadius,mainDir, maxRange, hitLayers);
        Debug.DrawRay(mainOrigin, mainDir * mainHit.distance, Color.red, 0.1f);  // ALWAYS draws
        //Debug.Log($"Main hit: {mainHit.collider?.name ?? "NOTHING"}");

        // SIDES - Always draw + log
        for (int i = 0; i < sideFirePoints.Length; i++)
        {
            if (sideFirePoints[i] == null) continue;
        
            Transform firePoint = sideFirePoints[i];
            Vector2 origin = firePoint.position;
        
            Debug.DrawRay(origin, mainDir * mainHit.distance, Color.yellow, 0.1f);  // ALWAYS draws
            //Debug.Log($"Side {i} hit: {hit.collider?.name ?? "NOTHING"}");
        }
    }
    
    public void ApplyLethality()
    {

        foreach(Vector3Int cell in mainCells)
        {
            HeatMap.Instance.heatmap[cell].cellLethalityScore += mainRayLethalityScore;
        }

        for (int i = 0; i < sideFirePointsLength; i++)
        {
            foreach(Vector3Int cell in sideCells[i])
            {
                HeatMap.Instance.heatmap[cell].cellLethalityScore += sideRayLethalityScore;
            }
        }
    }

    List<Vector3Int> GetRayCells(Vector2 origin, float maxDistance)
    {
        List<Vector3Int> cells = new();
        Vector3Int prevCell = HeatMap.Instance.grid.WorldToCell(origin);
    
        float distance = 0f;
        while (distance < maxDistance)
        {
            Vector2 dir = direction.right.normalized;
            Vector2 point = origin + dir * (distance + 0.3f);
            Vector3Int currentCell = HeatMap.Instance.grid.WorldToCell(point);
        
            if (currentCell != prevCell && HeatMap.Instance.heatmap.ContainsKey(currentCell))
            {
                cells.Add(currentCell);
                prevCell = currentCell;
            }
        
            distance += HeatMap.Instance.grid.cellSize.x * 0.8f;
        }
    
        return cells;
    }
    public float GetNormalizedAverageLethalityScore()
    {
        float totalLethalityScore = 0;
        int nrOfCells = mainCells.Count;

        foreach(Vector3Int cell in mainCells)
        {
            totalLethalityScore += HeatMap.Instance.heatmap[cell].cellLethalityScore;
        }

        for (int i = 0; i < sideFirePointsLength; i++)
        {
            nrOfCells += sideCells[i].Count;
            foreach(Vector3Int cell in sideCells[i])
            {
                totalLethalityScore += HeatMap.Instance.heatmap[cell].cellLethalityScore;
            }
        }
        if (nrOfCells == 0) return 0;
        return totalLethalityScore/(nrOfCells * maxAllowedLethalityScore);
    }

    public int GetTotalLethalityScore()
    {
        int totalScore = 0;
        
        foreach(Vector3Int cell in mainCells)
        {
            totalScore += HeatMap.Instance.heatmap[cell].cellLethalityScore;
        }

        // SIDE rays (same length)
        for (int i = 0; i < sideFirePointsLength; i++)
        {
            foreach(Vector3Int cell in sideCells[i])
            {
                totalScore += HeatMap.Instance.heatmap[cell].cellLethalityScore;
            }
        }
    
        return totalScore;
    }
    // 0th entry of totalvisits is current round, 1st entry is previous round, 2nd entry is 2 rounds ago
    //round starts with 0, in round 1 is where first items get placed
    public int[] GetTotalCellVisits()
    {
        int[] totalVisits = {0,0,0};
        int currentRoundIndex = PlayTestDataManager.Instance.roundIndex.Value;
        
        foreach(Vector3Int cell in mainCells)
        {
            totalVisits[0] += HeatMap.Instance.heatmap[cell].cellVisits[currentRoundIndex-1].Value;
            
            if (currentRoundIndex > 1)
                totalVisits[1] += HeatMap.Instance.heatmap[cell].cellVisits[currentRoundIndex-2].Value;
            if (currentRoundIndex > 2)
                totalVisits[2] += HeatMap.Instance.heatmap[cell].cellVisits[currentRoundIndex-3].Value;
        }

        for (int i = 0; i < sideFirePointsLength; i++)
        {

            foreach (Vector3Int cell in sideCells[i])
            {
                totalVisits[0] += HeatMap.Instance.heatmap[cell].cellVisits[currentRoundIndex-1].Value;

                if (currentRoundIndex > 1)
                    totalVisits[1] += HeatMap.Instance.heatmap[cell].cellVisits[currentRoundIndex - 2].Value;

                if (currentRoundIndex > 2)
                    totalVisits[2] += HeatMap.Instance.heatmap[cell].cellVisits[currentRoundIndex - 3].Value;
            }
        }
    
        return totalVisits;
    }


    public int GetOwnLethalityScore()
    {
        return mainRayLethalityScore;
    }

    public int GetMaxLethalityScore()
    {
        int maxScore = 0;
        
        foreach(Vector3Int cell in mainCells)
        {
            if(HeatMap.Instance.heatmap[cell].cellLethalityScore > maxScore)
                maxScore = HeatMap.Instance.heatmap[cell].cellLethalityScore;
        }

        for (int i = 0; i < sideFirePointsLength; i++)
        {

            foreach(Vector3Int cell in sideCells[i])
            {
                if(HeatMap.Instance.heatmap[cell].cellLethalityScore > maxScore)
                    maxScore = HeatMap.Instance.heatmap[cell].cellLethalityScore;            }
        }
    
        return maxScore;
    }
    
    public int GetHowManyCellsWouldBeAboveMaxLethality()
    {
        int counter = 0;
        
        foreach(Vector3Int cell in mainCells)
        {
            if (HeatMap.Instance.heatmap[cell].cellLethalityScore + mainRayLethalityScore > maxAllowedLethalityScore)
                counter++;
            
            // means we hit a spike, is here, because the ray ends when hitting
            // the spike sprite and then it would always be only a small number of
            // cells with high lethality, even though it goes through the spike
            if (HeatMap.Instance.heatmap[cell].cellLethalityScore > 90)
            {
                return 9000;
            }
        }

        for (int i = 0; i < sideFirePointsLength; i++)
        {

            foreach(Vector3Int cell in sideCells[i])
            {
                if (HeatMap.Instance.heatmap[cell].cellLethalityScore + mainRayLethalityScore > maxAllowedLethalityScore)
                    counter++;
                if (HeatMap.Instance.heatmap[cell].cellLethalityScore > 90)
                {
                    return 9000;
                }
            }
        }
    
        //Debug.Log("Cells above max lethality threshold: " + counter);
        return counter;
    }

    
    

    public void UpdateHitCells()
    {
        Vector2 mainOrigin = mainFirePoint.position;
        Vector2 mainDir = direction.right.normalized;
        if (transform.localScale.x < 0f)
        {
            mainDir.y = -mainDir.y;
        }
        RaycastHit2D mainHit = Physics2D.CircleCast(mainOrigin, circleCastRadius,mainDir, maxRange, hitLayers);
        mainRayActualLength = mainHit.collider ? mainHit.distance : maxRange;
        mainCells = GetRayCells(mainOrigin, mainRayActualLength);
        for (int i = 0; i < sideFirePointsLength; i++)
        {
            Transform fp = sideFirePoints[i];
            Vector2 origin = fp.position;
            Vector2 dir = direction.right.normalized;
            if (transform.localScale.x < 0f)
            {
                dir.y = dir.y;
            }
            RaycastHit2D hit = Physics2D.CircleCast(mainOrigin, circleCastRadius,mainDir, mainRayActualLength, hitLayers);
            Physics.SyncTransforms();

            sideRayActualLengths[i] = hit.collider ? hit.distance : mainRayActualLength;
        
            sideCells[i] = GetRayCells(origin, sideRayActualLengths[i]);
        }
        //Debug.Log("mainrayactual length: " + mainRayActualLength);
    }
    
    public float GetNormalizedAttackRangeUtilizationScore()
    {
        //Debug.Log("mainRayActualLength: " + mainRayActualLength + " maxRange: " + maxRange);
        return mainRayActualLength/maxRange;
    }


    public void Reset()
    {
        mainCells.Clear();
        for (int i = 0; i < sideFirePointsLength; i++)
        {
            sideCells[i].Clear();
        }
    }
}
