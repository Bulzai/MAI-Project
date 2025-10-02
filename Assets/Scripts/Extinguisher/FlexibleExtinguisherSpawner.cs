using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FlexibleExtinguisherSpawner : MonoBehaviour
{
    [Header("Prefabs & Spawn Points")]
    [SerializeField] private GameObject extinguisherPrefab;
    [SerializeField] private Transform spawnPointsParent;
    [SerializeField] private Transform extinguishersContainer;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnIntervalSeconds = 10f;
    [SerializeField] private float destroyAfterSeconds = 10f;
    [SerializeField] private int maxExtinguishers = 10;


    [Header("Circle Check")]
    [SerializeField] private int circleRadius = 4; // in cells, adjustable in inspector



    [Header("Placement Parameters")]
    int downHeight = 5, downBase = 7;    // Down triangle
    int sideHeight = 4, sideBase = 7;    // Left + Right triangles

    [SerializeField] private float jumpHeightWorld = 4f;
    [SerializeField] private float checkHeight = 4f;      // vertical radius of ellipse
    [SerializeField] private float checkLength = 8f;      // horizontal radius of ellipse
    [SerializeField] private int maxAttempts = 2;

    [Header("Grid + Tilemap")]
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap platformMap;
    [SerializeField] private TileBase validPlatformTile; // which tile color counts as a platform
    [SerializeField] private GridPlacementSystem gridPlacementSystem;


    [SerializeField] private KeyCode debugKey = KeyCode.H; // configurable in Inspector

    [Header("Spawn Area")]

    [SerializeField] private BoxCollider2D spawnZone;


    private readonly List<Transform> _spawnPoints = new List<Transform>();
    private Coroutine _spawnRoutine;
    private int _spawnedCount;
    private HashSet<Vector3Int> platformCells;

    private void Awake()
    {
        foreach (Transform t in spawnPointsParent)
            _spawnPoints.Add(t);
        platformCells = new HashSet<Vector3Int>();
        foreach (var pos in platformMap.cellBounds.allPositionsWithin)
        {
            if (platformMap.GetTile(pos) == validPlatformTile)
                platformCells.Add(pos);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += BeginSpawning;
        GameEvents.OnMainGameStateExited += StopSpawning;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= BeginSpawning;
        GameEvents.OnMainGameStateExited -= StopSpawning;
    }

    private void BeginSpawning()
    {
        _spawnedCount = 0;
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _spawnRoutine = StartCoroutine(SpawnExtinguishers());
    }

    private void StopSpawning()
    {
        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);
        _spawnRoutine = null;
    }

    private IEnumerator SpawnExtinguishers()
    {
        while (_spawnedCount < maxExtinguishers)
        {
            SpawnOne();
            _spawnedCount++;
            yield return new WaitForSeconds(spawnIntervalSeconds);
        }
    }

    private void SpawnOne()
    {
        StartCoroutine(TryFindValidSpawn((spawnPos) =>
        {
            if (spawnPos != Vector3.zero)
            {
                var go = Instantiate(extinguisherPrefab, spawnPos, extinguisherPrefab.transform.rotation, extinguishersContainer);
                Destroy(go, destroyAfterSeconds);
            }
            else
            {
                UnityEngine.Debug.Log("Fallback spawn point");
                if (_spawnPoints.Count == 0) return;
                int idx = Random.Range(0, _spawnPoints.Count);
                var point = _spawnPoints[idx];
                var go = Instantiate(extinguisherPrefab, point.position, extinguisherPrefab.transform.rotation, extinguishersContainer);
                Destroy(go, destroyAfterSeconds);
            }
        }));
    }



    private IEnumerator TryFindValidSpawn(System.Action<Vector3> onComplete)
    {
        Bounds b = spawnZone.bounds;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(b.min.x, b.max.x);
            float y = Random.Range(b.min.y, b.max.y);

            Vector3 candidateWorld = new Vector3(x, y, 0);
            Vector3Int randCell = grid.WorldToCell(candidateWorld);

            // ✅ Clear debug grid before drawing
            gridPlacementSystem.TempTilemap.ClearAllTiles();

            // 🔵 Draw the half-ellipses for this candidate
            DebugDrawTriangles(candidateWorld, downHeight, downBase, sideHeight, sideBase);
            //DebugDrawCircle(candidateWorld, circleRadius, TileType.Yellow);

            // 🔴 Highlight the candidate cell itself
            gridPlacementSystem.HighlightCells(new[] { randCell }, TileType.Red);

            // ⏸ Pause the game
            Time.timeScale = 0f;
            UnityEngine.Debug.Log($"Paused at attempt {attempt}, press Space to continue...");

            // 🚀 Wait until resume key is pressed
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }

            // ▶ Resume game
            Time.timeScale = 1f;

            // Now run the check
            if (HasValidTriangles(candidateWorld, downHeight, downBase, sideHeight, sideBase))
            //if (HasValidTriangles(candidateWorld, downHeight, downBase, sideHeight, sideBase) &&
             //   IsCircleAreaClear(candidateWorld, circleRadius))
            {
                UnityEngine.Debug.Log("Both halves above have at least 1 platform");
                Vector3 spawnPos = grid.CellToWorld(randCell) + grid.cellSize / 2f;
                onComplete?.Invoke(spawnPos); // ✅ return via callback
                yield break; // stop coroutine
            }
        }

        // no valid position found
        onComplete?.Invoke(Vector3.zero);
    }
    private bool HasValidTriangles(Vector3 worldOrigin, int downHeight, int downBase, int sideHeight, int sideBase)
    {
        Vector3Int centerCell = grid.WorldToCell(worldOrigin);

        // ❌ Make sure spawn center is empty
        if (platformCells.Contains(centerCell))
            return false;

        // Downward triangle (just needs one platform anywhere inside)
        bool down = CheckTriangleAny(centerCell, Vector2Int.down, downHeight, downBase);

        // Left / Right triangles (require proper wall structure)
        bool left = CheckTriangleWall(centerCell, Vector2Int.left, sideHeight, sideBase);
        bool right = CheckTriangleWall(centerCell, Vector2Int.right, sideHeight, sideBase);

        return down || left || right;
    }

    /// Simple check: any platform inside the triangle
    private bool CheckTriangleAny(Vector3Int origin, Vector2Int dir, int height, int baseLength)
    {
        for (int h = 0; h < height; h++)
        {
            int halfWidth = Mathf.RoundToInt((baseLength / 2f) * (h / (float)height));

            for (int dx = -halfWidth; dx <= halfWidth; dx++)
            {
                Vector3Int cell = origin + new Vector3Int(dir.x * h + (dir.y != 0 ? dx : 0),
                                                          dir.y * h + (dir.x != 0 ? dx : 0), 0);

                if (platformCells.Contains(cell))
                    return true;
            }
        }
        return false;
    }

    /// Wall check: bottom row must be full OR at least 3 vertical in a row are platform
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

                // Collect bottom row for later full-base check
                if (h == height - 1) bottomRow.Add(cell);
            }
        }

        // ✅ Condition 1: full base row is platform
        bool fullBase = bottomRow.Count > 0 && bottomRow.TrueForAll(c => platformCells.Contains(c));
        if (fullBase) return true;

        // ✅ Condition 2: at least 3 consecutive vertical cells are platform
        int streak = 0;
        for (int i = 0; i < height; i++)
        {
            Vector3Int wallCell = origin + new Vector3Int(dir.x * i, dir.y * i, 0);

            if (platformCells.Contains(wallCell))
            {
                streak++;
                if (streak >= 3) return true;
            }
            else
            {
                streak = 0; // reset if broken
            }
        }

        return false;
    }

    public void DebugDrawTriangles(Vector3 worldOrigin, int downHeight, int downBase, int sideHeight, int sideBase)
    {
        Vector3Int centerCell = grid.WorldToCell(worldOrigin);

        // 🔴 Center = spawn position
        gridPlacementSystem.HighlightCells(new[] { centerCell }, TileType.Red);

        // Down triangle
        DrawTriangle(centerCell, Vector2Int.down, downHeight, downBase, TileType.Blue);

        // Left triangle
        DrawTriangle(centerCell, Vector2Int.left, sideHeight, sideBase, TileType.White);

        // Right triangle
        DrawTriangle(centerCell, Vector2Int.right, sideHeight, sideBase, TileType.Red);
    }

    private void DrawTriangle(Vector3Int origin, Vector2Int dir, int height, int baseLength, TileType color)
    {
        for (int h = 0; h < height; h++)
        {
            int halfWidth = Mathf.RoundToInt((baseLength / 2f) * (h / (float)height));

            for (int dx = -halfWidth; dx <= halfWidth; dx++)
            {
                Vector3Int cell = origin + new Vector3Int(dir.x * h + (dir.y != 0 ? dx : 0),
                                                          dir.y * h + (dir.x != 0 ? dx : 0), 0);
                gridPlacementSystem.HighlightCells(new[] { cell }, color);
            }
        }
    }


    /// Check if a circle around the spawn point is clear of platform tiles
    private bool IsCircleAreaClear(Vector3 worldOrigin, int radius)
    {
        Vector3Int centerCell = grid.WorldToCell(worldOrigin);

        // Center must not be platform
        if (platformCells.Contains(centerCell))
            return false;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy <= radius * radius) // inside circle
                {
                    Vector3Int checkCell = centerCell + new Vector3Int(dx, dy, 0);

                    if (platformCells.Contains(checkCell))
                        return false; // found a platform inside circle
                }
            }
        }

        return true; // circle is clear
    }




    /// Draw a debug circle around the candidate
    public void DebugDrawCircle(Vector3 worldOrigin, int radius, TileType color)
    {
        Vector3Int centerCell = grid.WorldToCell(worldOrigin);

        // Highlight center
        gridPlacementSystem.HighlightCells(new[] { centerCell }, TileType.Red);

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy <= radius * radius) // inside circle
                {
                    Vector3Int cell = centerCell + new Vector3Int(dx, dy, 0);
                    gridPlacementSystem.HighlightCells(new[] { cell }, color);
                }
            }
        }
    }

}
