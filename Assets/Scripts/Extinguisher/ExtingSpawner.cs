using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class ExtingSpawner : MonoBehaviour
{
    [Header("Grid Reference")]
    [SerializeField] private GridPlacementSystem gridPlacement;


    [Header("Spawn Settings")]
    [SerializeField] private int triangleSpawns = 7;
    [SerializeField] private int circleSpawns = 3;
    [SerializeField] private int attemptsPerSpawn = 50;
    [SerializeField] private float spawnInterval = 15f;
    [SerializeField] private int extinguisherSpacing = 5;

    [Header("Prefabs & Containers")]
    [SerializeField] private GameObject extinguisherPrefab;
    [SerializeField] private GameObject previewPrefab; // sprite with number
    [SerializeField] private GameObject CircleExtinguisherPrefab;
    [SerializeField] private GameObject CirclePreviewPrefab; // sprite with number
    [SerializeField] private Transform extinguisherContainer;
    [SerializeField] private Transform previewContainer;
    private int maxPreviewCount;
    private List<Vector3Int> finalSpawnCells = new();   // use cells for easy tile coloring
    private List<GameObject> previewMarkers = new();    // spawned preview objects
    private bool spawnPositionsPrepared = false;


    [Header("Triangle Parameters")]
    [SerializeField] private int downHeight = 5;
    [SerializeField] private int downBase = 7;
    [SerializeField] private int sideHeight = 4;
    [SerializeField] private int sideBase = 7;

    [Header("Circle Parameters")]
    [SerializeField] private int circleRadius = 5;

    [Header("Grid + Tilemap")]
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap platformMap;
    [SerializeField] private TileBase validPlatformTile;
    [SerializeField] private TileBase validExtinguisherTile;

    [SerializeField] private BoxCollider2D spawnZone;


    [Header("UI")]
    [SerializeField] private GameObject loadingScreen; // assign a loading overlay in inspector


    private HashSet<Vector3Int> extinguisherCells = new();
    private HashSet<Vector3Int> platformCells;
    private List<Vector3> finalSpawnPositions = new List<Vector3>();
    private Coroutine _spawnRoutine;
    private int _nextIndex = 0;
    private bool _advanceRequested = false;
    private List<bool> spawnIsTriangle = new(); // true = triangle extinguisher, false = circle extinguisher

    // ===============================================================
    // UNITY LIFECYCLE
    // ===============================================================
    
    private void OnDestroy()
    {
        GameEvents.OnSurpriseBoxStateEntered -= PrepareSpawnPositions;
        GameEvents.OnPlaceItemStateEntered -= ShowPreviews;
        GameEvents.OnPlaceItemStateEntered -= MarkExtinguisherTiles;
        GameEvents.OnMainGameStateEntered -= HidePreviewsAndUnblockTiles;
        GameEvents.OnMainGameStateEntered -= BeginSpawning;
        GameEvents.OnMainGameStateExited -= StopSpawning;
        GameEvents.OnMainGameStateExited -= DisableExtinguisherContainer;
        GameEvents.OnMainGameStateEntered -= EnableExtinguisherContainer;
        GameEvents.OnFinalScoreStateEntered -= ResetExtinguisherSpawns;
    }

    private void Awake()
    {
        GameEvents.OnSurpriseBoxStateEntered += PrepareSpawnPositions;
        GameEvents.OnPlaceItemStateEntered += ShowPreviews;
        GameEvents.OnPlaceItemStateEntered += MarkExtinguisherTiles;
        GameEvents.OnMainGameStateEntered += HidePreviewsAndUnblockTiles;
        GameEvents.OnMainGameStateEntered += BeginSpawning;
        GameEvents.OnMainGameStateEntered += EnableExtinguisherContainer;
        GameEvents.OnMainGameStateExited += StopSpawning;
        GameEvents.OnMainGameStateExited += DisableExtinguisherContainer;
        GameEvents.OnFinalScoreStateEntered += ResetExtinguisherSpawns;
        
        platformCells = new HashSet<Vector3Int>();
        foreach (var pos in platformMap.cellBounds.allPositionsWithin)
        {
            if (platformMap.GetTile(pos) == validPlatformTile)
                platformCells.Add(pos);
        }
        // auto-sync preview count with configured spawn counts
        maxPreviewCount = triangleSpawns + circleSpawns;

    }


    // ----------------------------------------------------------
    // GAME FLOW
    // ----------------------------------------------------------
    private void BeginSpawning()
    {
        HidePreviewsAndUnblockTiles(); // ensure previews are gone

        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _spawnRoutine = StartCoroutine(SpawnLoopRoutine());
    }

    private void StopSpawning()
    {
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _spawnRoutine = null;
    }



    // ===============================================================
    // PHASE 1: SEARCH
    // ===============================================================
    /*public void PrepareSpawnPositions()
    {
        if (spawnPositionsPrepared) return; // only do once per game

        finalSpawnCells.Clear();
        extinguisherCells.Clear();
        previewMarkers.Clear();

        Bounds b = spawnZone.bounds;

        int triangleCount = 0;
        int circleCount = 0;
        int total = Mathf.Min(maxPreviewCount, triangleSpawns + circleSpawns);

        for (int i = 0; i < total; i++)
        {
            // Alternate, but respect caps
            bool useTriangle = true; // use this instead of true for alternating: (i % 2 == 0);
            if (useTriangle && triangleCount >= triangleSpawns) useTriangle = false;
            if (!useTriangle && circleCount >= circleSpawns) useTriangle = true;

            // Find a spawn cell (in grid coords)
            Vector3Int cell = FindSpawnPosition(b, useTriangle);

            // Track cell for logic
            finalSpawnCells.Add(cell);
            extinguisherCells.Add(cell);
            spawnIsTriangle.Add(useTriangle);  // ✅ record type

            // Convert to world space for previews/spawning
            //Vector3 pos = grid.CellToWorld(cell) + grid.cellSize / 2f;

            // (optional) spawn preview right away here
            // var marker = Instantiate(previewPrefab, pos, Quaternion.identity, previewContainer);

            // Increment method counter
            if (useTriangle) triangleCount++;
            else circleCount++;
        }

        spawnPositionsPrepared = true;
    }
    */

    public void PrepareSpawnPositions()
    {
        if (spawnPositionsPrepared) return; // only do once per game

        finalSpawnCells.Clear();
        extinguisherCells.Clear();
        previewMarkers.Clear();
        spawnIsTriangle.Clear(); // ✅ reset list

        Bounds b = spawnZone.bounds;

        int total = Mathf.Min(maxPreviewCount, triangleSpawns + circleSpawns);

        // --- STEP 1️⃣: Find all circle spawn positions first ---
        int circleCount = 0;
        for (int i = 0; i < circleSpawns && finalSpawnCells.Count < total; i++)
        {
            Vector3Int cell = FindSpawnPosition(b, false);
            finalSpawnCells.Add(cell);
            extinguisherCells.Add(cell);
            spawnIsTriangle.Add(false);
            circleCount++;
        }

        // --- STEP 2️⃣: Then find triangle positions ---
        int triangleCount = 0;
        for (int i = 0; i < triangleSpawns && finalSpawnCells.Count < total; i++)
        {
            Vector3Int cell = FindSpawnPosition(b, true);
            finalSpawnCells.Add(cell);
            extinguisherCells.Add(cell);
            spawnIsTriangle.Add(true);
            triangleCount++;
        }

        // --- STEP 3️⃣: Reorder spawn list so triangles spawn first ---
        ReorderForSpawn();

        spawnPositionsPrepared = true;
    }

    private void ReorderForSpawn()
    {
        // Move all triangles to the front, circles after
        List<Vector3Int> triangles = new();
        List<Vector3Int> circles = new();
        List<bool> trianglesType = new();
        List<bool> circlesType = new();

        for (int i = 0; i < finalSpawnCells.Count; i++)
        {
            if (spawnIsTriangle[i])
            {
                triangles.Add(finalSpawnCells[i]);
                trianglesType.Add(true);
            }
            else
            {
                circles.Add(finalSpawnCells[i]);
                circlesType.Add(false);
            }
        }

        finalSpawnCells = new List<Vector3Int>(triangles);
        finalSpawnCells.AddRange(circles);

        spawnIsTriangle = new List<bool>(trianglesType);
        spawnIsTriangle.AddRange(circlesType);
    }


    private Vector3Int? TryFindSpawn(Bounds bounds, bool useTriangle, bool enforceSpacing)
    {
        for (int attempt = 0; attempt < attemptsPerSpawn; attempt++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);

            Vector3 candidateWorld = new Vector3(x, y, 0);
            Vector3Int candidateCell = grid.WorldToCell(candidateWorld);

            // skip platforms
            if (platformCells.Contains(candidateCell))
                continue;

            // skip cells already reserved for extinguishers
            if (extinguisherCells.Contains(candidateCell))
                continue;

            // geometry method check
            bool valid = useTriangle
                ? HasValidTriangles(candidateWorld, downHeight, downBase, sideHeight, sideBase)
                : IsCircleAreaClear(candidateWorld, circleRadius);

            if (!valid) continue;

            // spacing check
            if (enforceSpacing && IsExtinguisherNearby(candidateCell, extinguisherSpacing))
                continue;

            return candidateCell;
        }
        return null;
    }


    private Vector3Int FindSpawnPosition(Bounds bounds, bool preferTriangle)
    {
        // 1) Preferred method
        var cell = TryFindSpawn(bounds, preferTriangle, true);
        if (cell != null) return cell.Value;

        cell = TryFindSpawn(bounds, preferTriangle, false);
        if (cell != null) return cell.Value;

        // 2) Other method
        //bool other = !preferTriangle;
        //cell = TryFindSpawn(bounds, other, true);
        //if (cell != null) return cell.Value;

        //cell = TryFindSpawn(bounds, other, false);
        //if (cell != null) return cell.Value;

        // 3) Fallback random
        for (int attempt = 0; attempt < attemptsPerSpawn * 2; attempt++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            Vector3Int candidateCell = grid.WorldToCell(new Vector3(x, y, 0));

            if (platformCells.Contains(candidateCell)) continue;
            if (extinguisherCells.Contains(candidateCell)) continue;

            return candidateCell;
        }

        // worst case: just return a random cell (unsafe)
        return grid.WorldToCell(Vector3.zero);
    }


    // ----------------------------------------------------------
    // PREVIEWS & TILE BLOCKING
    // ----------------------------------------------------------
    private void ShowPreviews()
    {
        ClearPreviews();

        for (int i = 0; i < finalSpawnCells.Count; i++)
        {
            Vector3 pos = grid.CellToWorld(finalSpawnCells[i]) + grid.cellSize / 2f;
            GameObject prefabToUse = spawnIsTriangle[i] ? previewPrefab : CirclePreviewPrefab;
            var marker = Instantiate(prefabToUse, pos, Quaternion.identity, previewContainer);

            // set number text
            var text = marker.GetComponentInChildren<TextMesh>();
            if (text != null) text.text = (i + 1).ToString();

            previewMarkers.Add(marker);
        }
    }

    private void DisableExtinguisherContainer()
    {
        
        foreach (Transform child in extinguisherContainer) Destroy(child.gameObject);
        extinguisherContainer.gameObject.SetActive(false);
    }

    private void EnableExtinguisherContainer()
    {
        extinguisherContainer.gameObject.SetActive(true);
    }

    private void MarkExtinguisherTiles()
    {
        gridPlacement.OccupyCellsMainTilemap(finalSpawnCells, TileType.Red);
    }
    private void HidePreviewsAndUnblockTiles()
    {
        ClearPreviews();

        // reset tiles back to White
        //gridPlacement.OccupyCellsMainTilemap(finalSpawnCells, TileType.White);
    }

    private void ClearPreviews()
    {
        foreach (var go in previewMarkers) Destroy(go);
        previewMarkers.Clear();
    }
    // ===============================================================
    // PHASE 3: SPAWNING LOOP
    // ===============================================================

    private IEnumerator SpawnLoopRoutine()
    {
        while (!spawnPositionsPrepared)
        {
            if (loadingScreen != null) loadingScreen.SetActive(true);
            yield return null;
        }
        if (loadingScreen != null) loadingScreen.SetActive(false);

        float timer = 0f; // force first spawn immediately
        _nextIndex = 0;

        while (true)
        {
            // time to spawn, or pickup requested immediate advance
            if (timer <= 0f || _advanceRequested)
            {
                _advanceRequested = false;

                SpawnExtinguisherAtIndex(_nextIndex);
                _nextIndex = (_nextIndex + 1) % finalSpawnCells.Count;

                timer = spawnInterval; // reset the “counter”
            }

            timer -= Time.deltaTime;
            yield return null; // frame-by-frame so we can interrupt
        }
    }


    public void SpawnExtinguisherAtIndex(int index)
    {
        Vector3Int cell = finalSpawnCells[index];
        Vector3 pos = grid.CellToWorld(cell) + grid.cellSize / 2f;

        GameObject prefabToUse = spawnIsTriangle[index] ? extinguisherPrefab : CircleExtinguisherPrefab;
        var exting = Instantiate(prefabToUse, pos, Quaternion.identity, extinguisherContainer);

        // Fade in + lifetime tied to spawnInterval
        var fade = exting.AddComponent<FadeInAndLife>();
        fade.Init(spawnInterval);

        // Hook up pickup
        var pickup = exting.GetComponent<ExtinguisherPickUp>();
        if (pickup != null) pickup.Init(this);
    }


    public void RequestAdvance()
    {
        _advanceRequested = true;
    }



    // ===============================================================
    // VALIDATION FUNCTIONS
    // ===============================================================

    private bool HasValidTriangles(Vector3 worldOrigin, int downHeight, int downBase, int sideHeight, int sideBase)
    {
        Vector3Int centerCell = grid.WorldToCell(worldOrigin);

        if (platformCells.Contains(centerCell)) return false;

        bool down = CheckTriangleAny(centerCell, Vector2Int.down, downHeight, downBase);
        bool left = CheckTriangleWall(centerCell, Vector2Int.left, sideHeight, sideBase);
        bool right = CheckTriangleWall(centerCell, Vector2Int.right, sideHeight, sideBase);

        return down || left || right;
    }

    private bool CheckTriangleAny(Vector3Int origin, Vector2Int dir, int height, int baseLength)
    {
        for (int h = 0; h < height; h++)
        {
            int halfWidth = Mathf.RoundToInt((baseLength / 2f) * (h / (float)height));
            for (int dx = -halfWidth; dx <= halfWidth; dx++)
            {
                Vector3Int cell = origin + new Vector3Int(dir.x * h + (dir.y != 0 ? dx : 0),
                                                          dir.y * h + (dir.x != 0 ? dx : 0), 0);
                if (platformCells.Contains(cell)) return true;
            }
        }
        return false;
    }

    private bool CheckTriangleWall(Vector3Int origin, Vector2Int dir, int height, int baseLength)
    {
        List<Vector3Int> bottomRow = new List<Vector3Int>();

        for (int h = 0; h < height; h++)
        {
            int halfWidth = Mathf.RoundToInt((baseLength / 2f) * (h / (float)height));
            for (int dx = -halfWidth; dx <= halfWidth; dx++)
            {
                Vector3Int cell = origin + new Vector3Int(dir.x * h + (dir.y != 0 ? dx : 0),
                                                          dir.y * h + (dir.x != 0 ? dx : 0), 0);
                if (h == height - 1) bottomRow.Add(cell);
            }
        }

        bool fullBase = bottomRow.Count > 0 && bottomRow.TrueForAll(c => platformCells.Contains(c));
        if (fullBase) return true;

        int streak = 0;
        for (int i = 0; i < height; i++)
        {
            Vector3Int wallCell = origin + new Vector3Int(dir.x * i, dir.y * i, 0);
            if (platformCells.Contains(wallCell))
            {
                streak++;
                if (streak >= 3) return true;
            }
            else streak = 0;
        }

        return false;
    }

    private bool IsCircleAreaClear(Vector3 worldOrigin, int radius)
    {
        Vector3Int centerCell = grid.WorldToCell(worldOrigin);
        if (platformCells.Contains(centerCell)) return false;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy <= radius * radius)
                {
                    Vector3Int checkCell = centerCell + new Vector3Int(dx, dy, 0);
                    if (platformCells.Contains(checkCell)) return false;
                }
            }
        }
        return true;
    }

    private bool IsExtinguisherNearby(Vector3Int centerCell, int radius)
    {
        foreach (var cell in extinguisherCells)
        {
            if (Vector3Int.Distance(cell, centerCell) <= radius)
                return true;
        }
        return false;
    }

    
    public void ResetExtinguisherSpawns()
    {
        spawnPositionsPrepared = false;
    }
}
