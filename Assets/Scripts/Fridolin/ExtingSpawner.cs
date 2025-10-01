using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class ExtingSpawner : MonoBehaviour
{
    [Header("Prefabs & Containers")]
    [SerializeField] private GameObject extinguisherPrefab;
    [SerializeField] private GameObject previewPrefab; // sprite with number
    [SerializeField] private Transform extinguisherContainer;
    [SerializeField] private Transform previewContainer;
    [SerializeField] private int maxPreviewCount = 10;
    private List<Vector3Int> finalSpawnCells = new();   // use cells for easy tile coloring
    private List<GameObject> previewMarkers = new();    // spawned preview objects
    private bool spawnPositionsPrepared = false;


    [Header("Grid Reference")]
    [SerializeField] private GridPlacementSystem gridPlacement;


    [Header("Spawn Settings")]
    [SerializeField] private int triangleSpawns = 7;
    [SerializeField] private int circleSpawns = 2;
    [SerializeField] private int attemptsPerSpawn = 50;
    [SerializeField] private float spawnInterval = 10f;

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

    private HashSet<Vector3Int> platformCells;
    private List<Vector3> finalSpawnPositions = new List<Vector3>();
    private Coroutine _spawnRoutine;
    private bool _positionsReady = false;

    // ===============================================================
    // UNITY LIFECYCLE
    // ===============================================================


    private void OnEnable()
    {
        GameEvents.OnSurpriseBoxStateEntered += PrepareSpawnPositions;
        GameEvents.OnPlaceItemStateEntered += ShowPreviews;
        GameEvents.OnPlaceItemStateEntered += MarkExtinguisherTiles;
        GameEvents.OnMainGameStateEntered += HidePreviewsAndUnblockTiles;
        GameEvents.OnMainGameStateEntered += BeginSpawning;
        GameEvents.OnMainGameStateExited += StopSpawning;
    }


    private void OnDisable()
    {
        GameEvents.OnSurpriseBoxStateEntered -= PrepareSpawnPositions;
        GameEvents.OnPlaceItemStateEntered -= ShowPreviews;
        GameEvents.OnPlaceItemStateEntered -= MarkExtinguisherTiles;
        GameEvents.OnMainGameStateEntered -= HidePreviewsAndUnblockTiles;
        GameEvents.OnMainGameStateEntered -= BeginSpawning;
        GameEvents.OnMainGameStateExited -= StopSpawning;
    }

    private void Awake()
    {
        platformCells = new HashSet<Vector3Int>();
        foreach (var pos in platformMap.cellBounds.allPositionsWithin)
        {
            if (platformMap.GetTile(pos) == validPlatformTile)
                platformCells.Add(pos);
        }
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

    public void PrepareSpawnPositions()
    {
        if (spawnPositionsPrepared) return; // only do once per game

        finalSpawnCells.Clear();
        previewMarkers.Clear();

        Bounds b = spawnZone.bounds;

        for (int i = 0; i < maxPreviewCount; i++)
        {
            // Alternate between triangle/circle or use your logic

            bool useTriangle = (i < triangleSpawns);
            Vector3 pos = FindSpawnPosition(b, useTriangle);
            Vector3Int cell = grid.WorldToCell(pos);
            finalSpawnCells.Add(cell);
        }

        spawnPositionsPrepared = true;

    }


    private IEnumerator PrepareSpawnPositionsRoutine()
    {
        if (loadingScreen != null) loadingScreen.SetActive(true);

        finalSpawnPositions.Clear();
        Bounds b = spawnZone.bounds;

        // Triangle spawns
        for (int i = 0; i < triangleSpawns; i++)
        {
            Vector3 pos = FindSpawnPosition(b, true);
            finalSpawnPositions.Add(pos);
            yield return null; // yield between spawns for smoother frame time
        }

        // Circle spawns
        for (int i = 0; i < circleSpawns; i++)
        {
            Vector3 pos = FindSpawnPosition(b, false);
            finalSpawnPositions.Add(pos);
            yield return null;
        }

        _positionsReady = true;
        if (loadingScreen != null) loadingScreen.SetActive(false);
    }

    private Vector3 FindSpawnPosition(Bounds bounds, bool useTriangleMethod)
    {
        Vector3 chosen = Vector3.zero;
        int count = 0;
        for (int attempt = 0; attempt < attemptsPerSpawn; attempt++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);

            Vector3 candidateWorld = new Vector3(x, y, 0);
            Vector3Int candidateCell = grid.WorldToCell(candidateWorld);

            if (platformCells.Contains(candidateCell))
                continue;

            bool valid = useTriangleMethod
                ? HasValidTriangles(candidateWorld, downHeight, downBase, sideHeight, sideBase)
                : IsCircleAreaClear(candidateWorld, circleRadius);

            if (valid)
            {
                chosen = grid.CellToWorld(candidateCell) + grid.cellSize / 2f;
                break;
            }

            chosen = grid.CellToWorld(candidateCell) + grid.cellSize / 2f; // fallback
            count = attempt;
        }
        if (!useTriangleMethod && count + 1 == attemptsPerSpawn) chosen = FindSpawnPosition(bounds, true);

        return chosen;
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
            var marker = Instantiate(previewPrefab, pos, Quaternion.identity, previewContainer);

            // set number text
            var text = marker.GetComponentInChildren<TextMesh>();
            if (text != null) text.text = (i + 1).ToString();

            previewMarkers.Add(marker);
        }
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
        // Wait until positions ready

        while (!spawnPositionsPrepared)
        {
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);

            }
            yield return null;
        }
        if (loadingScreen != null) loadingScreen.SetActive(false);

        int index = 0;
        while (true)
        {
            Vector3 pos = finalSpawnCells[index];
            Instantiate(extinguisherPrefab, pos, Quaternion.identity, extinguisherContainer);

            index = (index + 1) % finalSpawnCells.Count;
            yield return new WaitForSeconds(spawnInterval);
        }
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

    private bool IsNoExtinguisherNearby(Vector3 worldOrigin, int radius)
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
}
