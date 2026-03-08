using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementHeatmap : MonoBehaviour
{
    [Header("References")]
    public Grid grid;
    public BoxCollider2D boundsCollider;
    public Collider2D playerCollider;
    
    [Header("Config")]
    public float decayPerRound = 0.8f; // Recent rounds weigh more
    public int maxRounds = 10;
    
    private Dictionary<Vector3Int, CellData> heatmap = new(1024);
    private Vector3Int[] activeCells;
    private float currentDecay = 1f;
    private int currentRound = 0;
    private BoundsInt gridBounds;
    
    [System.Serializable]
    public struct CellData
    {
        public Vector2 sumDirection;
        public float totalWeight;
    }
    
    void Start()
    {
        PrecomputeActiveCells();
    }
    
    void FixedUpdate()
    {
        UpdateTouchedCells();
    }
    
    void PrecomputeActiveCells()
    {
        Vector3 boundsMin = boundsCollider.bounds.min;
        Vector3 boundsMax = boundsCollider.bounds.max;
        Vector3Int minCell = grid.WorldToCell(boundsMin);
        Vector3Int maxCell = grid.WorldToCell(boundsMax);
        gridBounds = new BoundsInt(minCell, new Vector3Int(maxCell.x - minCell.x + 1, maxCell.y - minCell.y + 1, 1));        List<Vector3Int> cells = new(gridBounds.size.x * gridBounds.size.y);
        
        for (int x = gridBounds.xMin; x < gridBounds.xMax; x++)
        {
            for (int y = gridBounds.yMin; y < gridBounds.yMax; y++)
            {
                Vector3 cellCenter = grid.CellToWorld(new Vector3Int(x, y, 0)) + grid.cellSize * 0.5f;
                if (boundsCollider.bounds.Contains(cellCenter))
                {
                    cells.Add(new Vector3Int(x, y, 0));
                    heatmap[cells[^1]] = new CellData(); // Initialize
                }
            }
        }
        
        activeCells = cells.ToArray();
        Debug.Log($"Precomputed {activeCells.Length} active cells");
    }
    
    void UpdateTouchedCells()
    {
        Bounds playerBounds = playerCollider.bounds;
        Vector3Int minCell = grid.WorldToCell(playerBounds.min);
        Vector3Int maxCell = grid.WorldToCell(playerBounds.max);
        
        // Clamp to active area
        minCell.x = Mathf.Max(minCell.x, gridBounds.xMin);
        minCell.y = Mathf.Max(minCell.y, gridBounds.yMin);
        maxCell.x = Mathf.Min(maxCell.x, gridBounds.xMax - 1);
        maxCell.y = Mathf.Min(maxCell.y, gridBounds.yMax - 1);
        
        Vector2 movementDir = GetPlayerMovementDirection();
        
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (heatmap.TryGetValue(cellPos, out CellData data))
                {
                    data.sumDirection += movementDir * currentDecay;
                    data.totalWeight += currentDecay;
                    heatmap[cellPos] = data;
                }
            }
        }
    }
    
    Vector2 GetPlayerMovementDirection()
    {
        // Use Rigidbody velocity or track prev position
        Rigidbody2D rb = playerCollider.attachedRigidbody;
        if (rb != null)
            return rb.velocity.normalized;
        
        return Vector2.zero; // Or implement position delta
    }
    
    public void NextRound()
    {
        currentRound++;
        currentDecay = Mathf.Pow(decayPerRound, maxRounds - currentRound);
        Debug.Log($"Round {currentRound}, decay: {currentDecay:F2}");
    }
    
    public float ScorePlacement(Vector3Int position, Vector2Int attackRange)
    {
        float score = 0f;
        int checks = 0;
        
        for (int dx = -attackRange.x; dx <= attackRange.x; dx++)
        {
            for (int dy = -attackRange.y; dy <= attackRange.y; dy++)
            {
                if (heatmap.TryGetValue(position + new Vector3Int(dx, dy, 0), out CellData data))
                {
                    float avgMag = data.sumDirection.magnitude / (data.totalWeight + 0.001f);
                    score += data.totalWeight * (1f - avgMag); // High visits, low direction bias
                    checks++;
                }
            }
        }
        
        return checks > 0 ? score / checks : 0f;
    }
    
    // For debugging/visualization
    public CellData GetCellData(Vector3Int cellPos) => heatmap.TryGetValue(cellPos, out var data) ? data : new CellData();
    public Vector3Int[] GetActiveCells() => activeCells;
}
