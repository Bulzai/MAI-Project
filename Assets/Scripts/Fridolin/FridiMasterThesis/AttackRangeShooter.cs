using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRangeShooter : MonoBehaviour, IAdaptiveItemScoreCalculator
{
    [SerializeField] private Transform[] sideFirePoints;
    [SerializeField] private Transform mainFirePoint;
    [SerializeField] private LayerMask hitLayers = -1;  // ← ADD THIS LINE
    [SerializeField] private Vector2 direction;
    private int mainRayLethalityScore = 50;
    private int sideRayLethalityScore = 30;
    private int maxRange = 50;
    private List<Vector3Int> mainCells = new List<Vector3Int>();
    private List<Vector3Int>[] sideCells = new List<Vector3Int>[sideFirePointsLength];
    private static int sideFirePointsLength = 4;
    private float mainRayActualLength;
    private float[] sideRayActualLengths = new float[sideFirePointsLength];
    
    
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
        Vector2 mainDir = direction;  // or .right if you want right
    
    
        RaycastHit2D mainHit = Physics2D.Raycast(mainOrigin, mainDir, maxRange, hitLayers);
        Debug.DrawRay(mainOrigin, mainDir * mainHit.distance, Color.red, 0.1f);  // ALWAYS draws
        //Debug.Log($"Main hit: {mainHit.collider?.name ?? "NOTHING"}");

        // SIDES - Always draw + log
        for (int i = 0; i < sideFirePoints.Length; i++)
        {
            if (sideFirePoints[i] == null) continue;
        
            Transform firePoint = sideFirePoints[i];
            Vector2 origin = firePoint.position;
        
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxRange, hitLayers);
            Debug.DrawRay(origin, direction * hit.distance, Color.yellow, 0.1f);  // ALWAYS draws
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

    List<Vector3Int> GetRayCells(Vector2 origin, Vector2 direction, float maxDistance)
    {
        List<Vector3Int> cells = new();
        Vector3Int prevCell = HeatMap.Instance.grid.WorldToCell(origin);
    
        float distance = 0f;
        while (distance < maxDistance)
        {
            Vector2 point = origin + direction.normalized * (distance + 0.3f);
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
    public float GetAverageLethalityScore()
    {
        float averageLethalityScore = 0;
        int nrOfCells = mainCells.Count;

        foreach(Vector3Int cell in mainCells)
        {
            averageLethalityScore += HeatMap.Instance.heatmap[cell].cellLethalityScore;
        }

        for (int i = 0; i < sideFirePointsLength; i++)
        {
            nrOfCells += sideCells[i].Count;
            foreach(Vector3Int cell in sideCells[i])
            {
                averageLethalityScore += HeatMap.Instance.heatmap[cell].cellLethalityScore;
            }
        }
        if (nrOfCells == 0) return 0;
        return averageLethalityScore/nrOfCells;
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

    public int GetTotalCellVisits()
    {
        int totalVisits = 0;
        
        foreach(Vector3Int cell in mainCells)
        {
            totalVisits += HeatMap.Instance.heatmap[cell].cellVisits[PlayTestDataManager.Instance.roundIndex.Value].Value;
        }

        for (int i = 0; i < sideFirePointsLength; i++)
        {

            foreach(Vector3Int cell in sideCells[i])
            {
                totalVisits += HeatMap.Instance.heatmap[cell].cellVisits[PlayTestDataManager.Instance.roundIndex.Value].Value;
            }
        }
    
        return totalVisits;
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

    public void UpdateHitCells()
    {
        Vector2 mainOrigin = mainFirePoint.position;
        Vector2 mainDir = direction;
        RaycastHit2D mainHit = Physics2D.Raycast(mainOrigin, mainDir, maxRange, hitLayers);
        mainRayActualLength = mainHit.collider ? mainHit.distance : maxRange;
        mainCells = GetRayCells(mainOrigin, mainDir, mainRayActualLength);
        for (int i = 0; i < sideFirePointsLength; i++)
        {
            Transform fp = sideFirePoints[i];
            Vector2 origin = fp.position;
            Vector2 dir = direction;
        
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, mainRayActualLength, hitLayers);
            Physics.SyncTransforms();

            sideRayActualLengths[i] = hit.collider ? hit.distance : mainRayActualLength;
        
            sideCells[i] = GetRayCells(origin, dir, sideRayActualLengths[i]);
        }
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
