using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMap : MonoBehaviour
{
    [Header("References")] public Grid grid;
    public BoxCollider2D placementBoundsCollider;
    public Collider2D playerCollider;

    [Header("Config")] public Dictionary<Vector3Int, CellData> heatmap = new(); // Your main data store
    public Vector3Int[] activeCells; // Fast iteration array
    public BoundsInt gridBounds; // Grid cell bounds

    private HashSet<Vector3Int> lastFrameCells = new();

    public static HeatMap Instance { get; private set; }

    private void Awake()
    {
        //init singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;

        }

        Instance = this;

        SinglePlayerScoreManager.OnPrepareNextPlaythrough += ResetAllCellData;
    }

    private void OnDestroy()
    {
        SinglePlayerScoreManager.OnPrepareNextPlaythrough -= ResetAllCellData;
    }

    private void Start()
    {
        InitializeGridCells(placementBoundsCollider);
    }

    private void FixedUpdate()
    {
        UpdatePlayerCells(playerCollider);
    }

    void InitializeGridCells(Collider2D boundsCollider)
    {

        // Step 1: Get world bounds and convert to grid cells
        Bounds worldBounds = boundsCollider.bounds;
        Vector3Int minCell = grid.WorldToCell(worldBounds.min);
        Vector3Int maxCell = grid.WorldToCell(worldBounds.max);

        // Step 2: Define grid bounds (inclusive min, exclusive max)
        gridBounds = new BoundsInt(minCell, new Vector3Int(
            maxCell.x - minCell.x + 1,
            maxCell.y - minCell.y + 1,
            1
        ));

        // Step 3: Find ALL cells whose centers are inside collider
        List<Vector3Int> cells = new(gridBounds.size.x * gridBounds.size.y);

        for (int x = gridBounds.xMin; x < gridBounds.xMax; x++)
        {
            for (int y = gridBounds.yMin; y < gridBounds.yMax; y++)
            {
                Vector2 cellPos = new Vector2(x, y);
                //Vector3 cellCenter = grid.CellToWorld(cellPos) + grid.cellSize * 0.5f;
                Vector3Int cell = grid.WorldToCell(cellPos);
                if (worldBounds.Contains(cellPos))
                {
                    cells.Add(cell);
                    heatmap[cell] = new CellData(); // Initialize CellData
                    //GridPlacementSystem.Instance.TakeCellBounds(cell);

                }
            }
        }

        activeCells = cells.ToArray();
        Debug.Log($"Initialized {activeCells.Length} active grid cells in bounds {gridBounds}");
    }

    public void ResetAllCellData()
    {
        foreach (Vector3Int cellPos in activeCells)
        {
            heatmap[cellPos].Clear();
        }
    }

    public void UpdatePlayerCells(Collider2D playerCollider)
    {
        Bounds playerBounds = playerCollider.bounds;
        playerBounds.Expand(0.1f); // Small margin for edge cases

        // Fast lookup set to avoid duplicate increments
        HashSet<Vector3Int> touchedCells = new HashSet<Vector3Int>();

        // Same logic as your InitializeGridCells - check cell centers
        Vector3Int minCell = grid.WorldToCell(playerBounds.min);
        Vector3Int maxCell = grid.WorldToCell(playerBounds.max);

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                // FIXED: Use actual WORLD cell center position
                Vector3Int cell = new Vector3Int(x, y, 0);
                Vector3 cellCenter = grid.CellToWorld(cell) + grid.cellSize * 0.5f;

                if (playerBounds.Contains(cellCenter) && heatmap.ContainsKey(cell))
                {
                    touchedCells.Add(cell);
                }
            }

        }

        // Find newly entered cells only
        HashSet<Vector3Int> currentCells = touchedCells; // From above logic

        HashSet<Vector3Int> enteredCells = new HashSet<Vector3Int>(currentCells);
        enteredCells.ExceptWith(lastFrameCells);

        foreach (Vector3Int cell in enteredCells)
        {
            if(PlayTestDataManager.Instance.roundIndex.Value >= heatmap[cell].cellVisits.Length) continue; // Safety check
            heatmap[cell].cellVisits[PlayTestDataManager.Instance.roundIndex.Value].Increment();
            PlayTestDataManager.Instance.roundDistanceTravelled.Increment();
            GridPlacementSystem.Instance.TakeCellBounds(cell);
            //Debug.Log(" lethality score of cell " + cell + " is now " + heatmap[cell].cellLethalityScore);
            //Debug.Log("Updated cell " + cell + " for round " + PlayTestDataManager.Instance.roundIndex.Value + " new count: " + heatmap[cell].cellVisits[PlayTestDataManager.Instance.roundIndex.Value].Value);
        }

        lastFrameCells = currentCells;

    }

    public int GetCoveredByAttackRangeCellCount()
    {
        int cellsHitByLethality = 0;
        foreach (CellData cell in heatmap.Values)
        {
            if (cell.cellLethalityScore > 0)
            {
                cellsHitByLethality++;
            }
        }
        return cellsHitByLethality;
    }
}


[System.Serializable]
public class CellData 
{
    public IntCounter[] cellVisits = new IntCounter[SinglePlayerScoreManager.Instance.MAX_ROUNDS];
    public int cellLethalityScore = 0;
    
    public CellData() 
    {
        for(int i = 0; i < cellVisits.Length; i++) cellVisits[i] = new IntCounter();
    }
    
    public void Clear() 
    {
        foreach(var counter in cellVisits) counter.Reset();
        cellLethalityScore = 0;
    }
}
