using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class AutomaticPlacement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(RunAutoPlacement());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        Debug.Log("ENTER AUTOMATIC PLACEMNT");

        StopAllCoroutines();
        StartCoroutine(RunAutoPlacement());
    }

    private void OnDisable()
    {
        Debug.Log("EXIT AUTOMATIC PLACEMNT");
    }

    private IEnumerator RunAutoPlacement()
    {
        GridPlacementSystem.Instance.ResetMainTileMap();

        if (ItemDisplay.Instance == null)
        {
            Debug.LogError("Manager not found!");
            yield break;
        }

        int playerCount = PlayerManager.Instance != null ? PlayerManager.Instance.playerCount : 1;

        // max = playercount, min = playercount - 1
        int itemsToPlace = Random.Range(playerCount - 1, playerCount);

        // do not place 0 items if only 1 player is active
        if (itemsToPlace < 1) itemsToPlace = 1;

        Debug.Log($"auto-placing {itemsToPlace} items for {playerCount} players.");

        // get item pool and valid cells
        List<GameObject> pool = ItemDisplay.Instance.getGeneralItemPool();
        List<Vector3Int> validCells = GetAllValidCells();

        bool placed = false;
        int attempts = 0;

        for (int i = 0; i < itemsToPlace; i++)
        {
            if (validCells.Count == 0 || pool.Count == 0) break;

            // get random prefab from pool
            GameObject prefab = pool[Random.Range(0, pool.Count)];
            GameObject spawnedItem = Instantiate(prefab, PlayerManager.Instance.itemContainer);
            GridItem gridItem = spawnedItem.GetComponent<GridItem>();

            if (gridItem == null)
            {
                Destroy(spawnedItem);
                continue;
            }

            attempts = 0;
            placed = false;

            while (!placed && attempts < 15)
            {
                attempts++;
                if (validCells.Count == 0)
                {
                    Debug.Log($"Valid Cell count = {validCells.Count}");
                    break;
                }

                // convert cell position to world position and item offset
                Vector3Int randomCell = validCells[Random.Range(0, validCells.Count)];
                Vector3 worldPosition = GridPlacementSystem.Instance.gridLayout.CellToWorld(randomCell)
                                        + (GridPlacementSystem.Instance.gridLayout.cellSize / 2f);

                spawnedItem.transform.position = worldPosition + gridItem.placementOffset;

                for (int r = 0; r < Random.Range(0, 4); r++)
                {
                    gridItem.RotateClockwise();
                }

                Physics2D.SyncTransforms();
                GridPlacementSystem.Instance.FollowItem(gridItem);

                if (gridItem.CanBePlaced())
                {
                    gridItem.Place();

                    // remove cell from local available list
                    foreach (var cell in GetOccupiedCellsForItem(gridItem))
                    {
                        // now taken
                        GridPlacementSystem.Instance.TakeCell(cell);
                        // remove from list of available spots for next item in loop
                        validCells.Remove(cell);
                    }

                    placed = true;
                }

            }

            // placement did not work so delete item
            if (!placed) Destroy(spawnedItem);
        }
        GameEvents.ChangeState(GameState.PlaceItemState);
    }

    private List<Vector3Int> GetAllValidCells()
    {
        List<Vector3Int> whiteTiles = new List<Vector3Int>();
        Tilemap mainMap = GridPlacementSystem.Instance.MainTilemap;

        // get bounds
        BoundsInt bounds = mainMap.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (GridPlacementSystem.Instance.CanTakeCell(pos))
            {
                whiteTiles.Add(pos);
            }
        }
        return whiteTiles;
    }

    // get which tiles are occupied
    private List<Vector3Int> GetOccupiedCellsForItem(GridItem item)
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        // get collider to see how much space it would take up
        Collider2D collider = item.GetComponent<Collider2D>();

        if (collider == null)
        {
            // default to 1x1 if no collider is found
            cells.Add(GridPlacementSystem.Instance.gridLayout.WorldToCell(item.transform.position - item.placementOffset));
            return cells;
        }

        // get bounds 
        Bounds area = collider.bounds;

        // convert min max corners of world bounds to tilemap cell coordinates
        Vector3Int minCell = GridPlacementSystem.Instance.gridLayout.WorldToCell(area.min);
        Vector3Int maxCell = GridPlacementSystem.Instance.gridLayout.WorldToCell(area.max);

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                cells.Add(new Vector3Int(x, y, 0));
            }
        }
        return cells;
    }
}

//using System.Collections;
//using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.Tilemaps;
//using UnityEngine.UI;

//public class AutomaticPlacement : MonoBehaviour
//{
//    void Start() { }
//    void Update() { }

//    private void OnEnable()
//    {
//        Debug.Log("ENTER AUTOMATIC PLACEMENT");
//        StopAllCoroutines();
//        StartCoroutine(RunAutoPlacement());
//    }

//    private void OnDisable()
//    {
//        Debug.Log("EXIT AUTOMATIC PLACEMENT");
//    }

//    private IEnumerator RunAutoPlacement()
//    {
//        GridPlacementSystem.Instance.ResetMainTileMap();

//        // Wait for managers and frame to settle
//        yield return new WaitForEndOfFrame();

//        if (ItemDisplay.Instance == null)
//        {
//            Debug.LogError("Manager not found!");
//            yield break;
//        }

//        int playerCount = PlayerManager.Instance != null ? PlayerManager.Instance.playerCount : 1;
//        int itemsToPlace = Random.Range(playerCount - 1, playerCount);
//        if (itemsToPlace < 1) itemsToPlace = 1;

//        Debug.Log($"auto-placing {itemsToPlace} items for {playerCount} players.");

//        List<GameObject> pool = ItemDisplay.Instance.getGeneralItemPool();
//        List<Vector3Int> validCells = GetAllValidCells();

//        // 3. Placement Loop
//        for (int i = 0; i < itemsToPlace; i++)
//        {
//            if (validCells.Count == 0 || pool.Count == 0) break;

//            GameObject prefab = pool[Random.Range(0, pool.Count)];
//            GameObject spawnedItem = Instantiate(prefab, PlayerManager.Instance.itemContainer);
//            GridItem gridItem = spawnedItem.GetComponent<GridItem>();

//            if (gridItem == null)
//            {
//                Destroy(spawnedItem);
//                continue;
//            }

//            bool placed = false;
//            int attempts = 0;

//            while (!placed && attempts < 15)
//            {
//                attempts++;
//                if (validCells.Count == 0) break;

//                // Pick a random grid coordinate
//                Vector3Int randomCell = validCells[Random.Range(0, validCells.Count)];
//                Vector3 worldPosition = GridPlacementSystem.Instance.gridLayout.CellToWorld(randomCell)
//                                        + (GridPlacementSystem.Instance.gridLayout.cellSize / 2f);

//                spawnedItem.transform.position = worldPosition + gridItem.placementOffset;

//                // Test all 4 possible rotations at this cell
//                bool foundValidRotation = false;
//                for (int rot = 0; rot < 4; rot++)
//                {
//                    // Let physics engine register the new position/rotation for raycasts
//                    yield return new WaitForFixedUpdate();

//                    GridPlacementSystem.Instance.FollowItem(gridItem);

//                    bool isValidPlacement = false;
//                    List<Vector3Int> placementPool = validCells;

//                    if (gridItem.isAttachable)
//                    {
//                        Collider2D col = gridItem.GetComponentInChildren<Collider2D>();

//                        if(col != null)
//    {
//                            Collider2D[] hits = Physics2D.OverlapPointAll(col.bounds.center);
//                            bool foundValidSurface = false;

//                            foreach (var hit in hits)
//                            {
//                                // Check if it's colliding with a valid target (e.g., Ice, Sticky surface, or Ground)
//                                if (hit.gameObject != spawnedItem && (hit.CompareTag("Ice") || hit.CompareTag("Sticky") || hit.CompareTag("GridItem")))
//                                {
//                                    foundValidSurface = true;
//                                    break;
//                                }
//                            }

//                            Debug.Log($" Attachment Check: foundValidSurface = {foundValidSurface} (Hits count: {hits.Length})");

//                            if (!foundValidSurface && hits.Length > 0)
//                            {
//                                foundValidSurface = true;
//                            }

//                            // 2. Set the flag based on the check (or force it true if spawning on a valid grid cell that supports it)
//                            gridItem.SupportedItemCanBePlaced = foundValidSurface;
//                        }
//                        else
//                        {
//                            // Fallback if no collider bounds found
//                            gridItem.SupportedItemCanBePlaced = true;
//                        }

//                        isValidPlacement = gridItem.SupportedItemCanBePlaced;
//                    }

//                    else
//                    {
//                        isValidPlacement = gridItem.CanBePlaced();
//                    }

//                    if (isValidPlacement)
//                    {
//                        // Replicate manual placement finalization steps from CursorController
//                        if (gridItem.isAttachable && gridItem.ifAttachableAttachHere != null)
//                        {
//                            spawnedItem.transform.SetParent(PlayerManager.Instance.itemContainer);
//                        }

//                        gridItem.Place();

//                        if (!gridItem.isAttachable)
//                        {
//                            spawnedItem.transform.SetParent(PlayerManager.Instance.itemContainer);
//                        }

//                        spawnedItem.layer = LayerMask.NameToLayer("Ground/Wall");

//                        // Clean up hover script if present, preventing scaling bugs
//                        var hoverHighlight = spawnedItem.GetComponent<HoverHighlight>();
//                        if (hoverHighlight != null)
//                        {
//                            hoverHighlight.RemoveHover();
//                            Destroy(hoverHighlight);
//                        }

//                        // Remove cells from available local list
//                        foreach (var cell in GetOccupiedCellsForItem(gridItem))
//                        {
//                            GridPlacementSystem.Instance.TakeCell(cell);
//                            validCells.Remove(cell);
//                        }

//                        foundValidRotation = true;
//                        placed = true;
//                        break;
//                    }

//                    // Rotate clockwise to test the next side orientation
//                    gridItem.RotateClockwise();
//                }

//                if (foundValidRotation) break;
//            }

//            // If placement failed across all cell attempts and rotations, clean it up
//            if (!placed)
//            {
//                Destroy(spawnedItem);
//            }
//        }

//        GameEvents.ChangeState(GameState.PlaceItemState);
//    }

//    private List<Vector3Int> GetAllValidCells()
//    {
//        List<Vector3Int> whiteTiles = new List<Vector3Int>();
//        Tilemap mainMap = GridPlacementSystem.Instance.MainTilemap;

//        BoundsInt bounds = mainMap.cellBounds;

//        foreach (var pos in bounds.allPositionsWithin)
//        {
//            if (GridPlacementSystem.Instance.CanTakeCell(pos))
//            {
//                whiteTiles.Add(pos);
//            }
//        }

//        Debug.Log(whiteTiles.Count + " valid cells found on tilemap.");

//        return whiteTiles;
//    }

//    private List<Vector3Int> GetOccupiedCellsForItem(GridItem item)
//    {
//        List<Vector3Int> cells = new List<Vector3Int>();
//        Collider2D collider = item.GetComponent<Collider2D>();

//        if (collider == null)
//        {
//            cells.Add(GridPlacementSystem.Instance.gridLayout.WorldToCell(item.transform.position - item.placementOffset));
//            return cells;
//        }

//        Bounds area = collider.bounds;
//        Vector3Int minCell = GridPlacementSystem.Instance.gridLayout.WorldToCell(area.min);
//        Vector3Int maxCell = GridPlacementSystem.Instance.gridLayout.WorldToCell(area.max);

//        for (int x = minCell.x; x <= maxCell.x; x++)
//        {
//            for (int y = minCell.y; y <= maxCell.y; y++)
//            {
//                cells.Add(new Vector3Int(x, y, 0));
//            }
//        }
//        return cells;
//    }
//}

////using System.Collections;
////using System.Collections.Generic;
////using UnityEngine;
////using UnityEngine.Tilemaps;

////public class AutomaticPlacement : MonoBehaviour
////{
////    void Start() { }
////    void Update() { }

////    private void OnEnable()
////    {
////        Debug.Log("ENTER AUTOMATIC PLACEMENT");
////        StopAllCoroutines();
////        StartCoroutine(RunAutoPlacement());

////        GameEvents.OnMainGameStateEntered += HandleMainGameStateEntered;
////    }

////    private void OnDisable()
////    {
////        Debug.Log("EXIT AUTOMATIC PLACEMENT");
////        GameEvents.OnMainGameStateEntered -= HandleMainGameStateEntered;
////    }

////    private void HandleMainGameStateEntered()
////    {
////        StopAllCoroutines();
////        StartCoroutine(RunAutoPlacement());
////    }

////    private IEnumerator RunAutoPlacement()
////    {
////        GridPlacementSystem.Instance.ResetMainTileMap();

////        yield return new WaitForEndOfFrame();

////        if (ItemDisplay.Instance == null)
////        {
////            Debug.LogError("Manager not found!");
////            yield break;
////        }

////        int playerCount = PlayerManager.Instance != null ? PlayerManager.Instance.playerCount : 1;
////        int itemsToPlace = Random.Range(playerCount - 1, playerCount);
////        if (itemsToPlace < 1) itemsToPlace = 1;

////        Debug.Log($"auto-placing {itemsToPlace} items for {playerCount} players.");

////        List<GameObject> pool = ItemDisplay.Instance.getGeneralItemPool();
////        List<Vector3Int> validCells = GetAllValidCells();

////        for (int i = 0; i < itemsToPlace; i++)
////        {
////            if (validCells.Count == 0 || pool.Count == 0) break;

////            GameObject prefab = pool[Random.Range(0, pool.Count)];
////            GameObject spawnedItem = Instantiate(prefab, PlayerManager.Instance.itemContainer);
////            GridItem gridItem = spawnedItem.GetComponent<GridItem>();

////            if (gridItem == null)
////            {
////                Destroy(spawnedItem);
////                continue;
////            }

////            // For attachable items, restrict the search pool to cells adjacent to platform tiles
////            // so it has a valid surface to attach to naturally.
////            List<Vector3Int> placementPool = validCells;
////            if (gridItem.isAttachable)
////            {
////                List<Vector3Int> surfaceCells = new List<Vector3Int>();
////                Tilemap mainMap = GridPlacementSystem.Instance.MainTilemap;

////                foreach (var cell in validCells)
////                {
////                    if (HasAdjacentTile(mainMap, cell))
////                    {
////                        surfaceCells.Add(cell);
////                    }
////                }

////                if (surfaceCells.Count > 0)
////                {
////                    placementPool = surfaceCells;
////                }
////            }

////            bool placed = false;
////            int attempts = 0;

////            // Keep trying random cells and rotations until a genuinely valid spot is found
////            while (!placed && attempts < 40)
////            {
////                attempts++;
////                if (placementPool.Count == 0) break;

////                Vector3Int randomCell = placementPool[Random.Range(0, placementPool.Count)];
////                Vector3 snappedWorld = GridPlacementSystem.Instance.gridLayout.CellToWorld(randomCell)
////                                       + (GridPlacementSystem.Instance.gridLayout.cellSize / 2f);

////                spawnedItem.transform.position = snappedWorld + gridItem.placementOffset;

////                bool foundValidRotation = false;
////                for (int rot = 0; rot < 4; rot++)
////                {
////                    Physics2D.SyncTransforms();

////                    GridPlacementSystem.Instance.FollowItem(gridItem);

////                    // Give physics a frame to catch up if needed
////                    yield return new WaitForFixedUpdate();
////                    Physics2D.SyncTransforms();

////                    bool isValidPlacement = false;

////                    if (gridItem.isAttachable)
////                    {
////                        isValidPlacement = gridItem.SupportedItemCanBePlaced;
////                    }
////                    else
////                    {
////                        isValidPlacement = gridItem.CanBePlaced();
////                    }

////                    if (isValidPlacement)
////                    {
////                        if (gridItem.isAttachable && gridItem.ifAttachableAttachHere != null)
////                        {
////                            spawnedItem.transform.SetParent(PlayerManager.Instance.itemContainer);
////                        }

////                        gridItem.Place();

////                        if (!gridItem.isAttachable)
////                        {
////                            spawnedItem.transform.SetParent(PlayerManager.Instance.itemContainer);
////                        }

////                        spawnedItem.layer = LayerMask.NameToLayer("Ground/Wall");

////                        var hoverHighlight = spawnedItem.GetComponent<HoverHighlight>();
////                        if (hoverHighlight != null)
////                        {
////                            hoverHighlight.RemoveHover();
////                            Destroy(hoverHighlight);
////                        }

////                        foreach (var cell in GetOccupiedCellsForItem(gridItem))
////                        {
////                            GridPlacementSystem.Instance.TakeCell(cell);
////                            validCells.Remove(cell);
////                            placementPool.Remove(cell);
////                        }

////                        foundValidRotation = true;
////                        placed = true;
////                        break;
////                    }

////                    gridItem.RotateClockwise();
////                }

////                if (foundValidRotation) break;
////            }

////            // If no genuinely valid placement could be found after all attempts, clean it up
////            if (!placed)
////            {
////                Destroy(spawnedItem);
////            }
////        }

////        GameEvents.ChangeState(GameState.PlaceItemState);
////    }

////    private List<Vector3Int> GetAllValidCells()
////    {
////        List<Vector3Int> whiteTiles = new List<Vector3Int>();
////        Tilemap mainMap = GridPlacementSystem.Instance.MainTilemap;
////        BoundsInt bounds = mainMap.cellBounds;

////        foreach (var pos in bounds.allPositionsWithin)
////        {
////            if (GridPlacementSystem.Instance.CanTakeCell(pos))
////            {
////                whiteTiles.Add(pos);
////            }
////        }
////        return whiteTiles;
////    }

////    private bool HasAdjacentTile(Tilemap tilemap, Vector3Int cellPos)
////    {
////        Vector3Int[] directions = {
////            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
////        };

////        foreach (var dir in directions)
////        {
////            if (tilemap.HasTile(cellPos + dir)) return true;
////        }
////        return false;
////    }

////    private List<Vector3Int> GetOccupiedCellsForItem(GridItem item)
////    {
////        List<Vector3Int> cells = new List<Vector3Int>();
////        Collider2D collider = item.GetComponent<Collider2D>();

////        if (collider == null)
////        {
////            cells.Add(GridPlacementSystem.Instance.gridLayout.WorldToCell(item.transform.position - item.placementOffset));
////            return cells;
////        }

////        Bounds area = collider.bounds;
////        Vector3Int minCell = GridPlacementSystem.Instance.gridLayout.WorldToCell(area.min);
////        Vector3Int maxCell = GridPlacementSystem.Instance.gridLayout.WorldToCell(area.max);

////        for (int x = minCell.x; x <= maxCell.x; x++)
////        {
////            for (int y = minCell.y; y <= maxCell.y; y++)
////            {
////                cells.Add(new Vector3Int(x, y, 0));
////            }
////        }
////        return cells;
////    }
////}

//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class AutomaticPlacement : MonoBehaviour
//{
//    void Start() { }
//    void Update() { }

//    private void OnEnable()
//    {
//        Debug.Log("ENTER AUTOMATIC PLACEMENT");
//        GameEvents.OnMainGameStateEntered += RunAutoPlacement;
//    }

//    private void OnDisable()
//    {
//        Debug.Log("EXIT AUTOMATIC PLACEMENT");
//        GameEvents.OnMainGameStateEntered -= RunAutoPlacement;
//    }

//    private void RunAutoPlacement()
//    {
//        // Ensure grid system and map exist before running
//        if (GridPlacementSystem.Instance == null || GridPlacementSystem.Instance.MainTilemap == null)
//        {
//            Debug.LogError("GridPlacementSystem or MainTilemap not found!");
//            return;
//        }

//        GridPlacementSystem.Instance.ResetMainTileMap();

//        if (ItemDisplay.Instance == null)
//        {
//            Debug.LogError("Manager not found!");
//            return;
//        }

//        int playerCount = PlayerManager.Instance != null ? PlayerManager.Instance.playerCount : 1;
//        int itemsToPlace = Random.Range(playerCount - 1, playerCount);
//        if (itemsToPlace < 1) itemsToPlace = 1;

//        Debug.Log($"auto-placing {itemsToPlace} items for {playerCount} players.");

//        List<GameObject> pool = ItemDisplay.Instance.getGeneralItemPool();
//        List<Vector3Int> validCells = GetAllValidCells();

//        for (int i = 0; i < itemsToPlace; i++)
//        {
//            if (validCells.Count == 0 || pool.Count == 0) break;

//            GameObject prefab = pool[Random.Range(0, pool.Count)];
//            GameObject spawnedItem = Instantiate(prefab, PlayerManager.Instance.itemContainer);
//            GridItem gridItem = spawnedItem.GetComponent<GridItem>();

//            if (gridItem == null)
//            {
//                Destroy(spawnedItem);
//                continue;
//            }

//            List<Vector3Int> placementPool = validCells;
//            if (gridItem.isAttachable)
//            {
//                List<Vector3Int> surfaceCells = new List<Vector3Int>();
//                Tilemap mainMap = GridPlacementSystem.Instance.MainTilemap;

//                foreach (var cell in validCells)
//                {
//                    if (HasAdjacentTile(mainMap, cell))
//                    {
//                        surfaceCells.Add(cell);
//                    }
//                }

//                if (surfaceCells.Count > 0)
//                {
//                    placementPool = surfaceCells;
//                }
//            }

//            bool placed = false;
//            int attempts = 0;

//            while (!placed && attempts < 40)
//            {
//                attempts++;
//                if (placementPool.Count == 0) break;

//                Vector3Int randomCell = placementPool[Random.Range(0, placementPool.Count)];
//                Vector3 snappedWorld = GridPlacementSystem.Instance.gridLayout.CellToWorld(randomCell)
//                                       + (GridPlacementSystem.Instance.gridLayout.cellSize / 2f);

//                spawnedItem.transform.position = snappedWorld + gridItem.placementOffset;

//                bool foundValidRotation = false;
//                for (int rot = 0; rot < 4; rot++)
//                {
//                    // Force physics and follow item state synchronously
//                    Physics2D.SyncTransforms();
//                    GridPlacementSystem.Instance.FollowItem(gridItem);
//                    Physics2D.SyncTransforms();

//                    bool isValidPlacement = false;

//                    if (gridItem.isAttachable)
//                    {
//                        isValidPlacement = gridItem.SupportedItemCanBePlaced;
//                    }
//                    else
//                    {
//                        isValidPlacement = gridItem.CanBePlaced();
//                    }

//                    if (isValidPlacement)
//                    {
//                        if (gridItem.isAttachable && gridItem.ifAttachableAttachHere != null)
//                        {
//                            spawnedItem.transform.SetParent(PlayerManager.Instance.itemContainer);
//                        }

//                        gridItem.Place();

//                        if (!gridItem.isAttachable)
//                        {
//                            spawnedItem.transform.SetParent(PlayerManager.Instance.itemContainer);
//                        }

//                        spawnedItem.layer = LayerMask.NameToLayer("Ground/Wall");

//                        var hoverHighlight = spawnedItem.GetComponent<HoverHighlight>();
//                        if (hoverHighlight != null)
//                        {
//                            hoverHighlight.RemoveHover();
//                            Destroy(hoverHighlight);
//                        }

//                        foreach (var cell in GetOccupiedCellsForItem(gridItem))
//                        {
//                            GridPlacementSystem.Instance.TakeCell(cell);
//                            validCells.Remove(cell);
//                            placementPool.Remove(cell);
//                        }

//                        foundValidRotation = true;
//                        placed = true;
//                        break;
//                    }

//                    gridItem.RotateClockwise();
//                }

//                if (foundValidRotation) break;
//            }

//            if (!placed)
//            {
//                Destroy(spawnedItem);
//            }
//        }

//        GameEvents.ChangeState(GameState.PlaceItemState);
//    }

//    private List<Vector3Int> GetAllValidCells()
//    {
//        List<Vector3Int> whiteTiles = new List<Vector3Int>();
//        Tilemap mainMap = GridPlacementSystem.Instance.MainTilemap;
//        BoundsInt bounds = mainMap.cellBounds;

//        foreach (var pos in bounds.allPositionsWithin)
//        {
//            if (GridPlacementSystem.Instance.CanTakeCell(pos))
//            {
//                whiteTiles.Add(pos);
//            }
//        }
//        return whiteTiles;
//    }

//    private bool HasAdjacentTile(Tilemap tilemap, Vector3Int cellPos)
//    {
//        Vector3Int[] directions = {
//            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
//        };

//        foreach (var dir in directions)
//        {
//            if (tilemap.HasTile(cellPos + dir)) return true;
//        }
//        return false;
//    }

//    private List<Vector3Int> GetOccupiedCellsForItem(GridItem item)
//    {
//        List<Vector3Int> cells = new List<Vector3Int>();
//        Collider2D collider = item.GetComponent<Collider2D>();

//        if (collider == null)
//        {
//            cells.Add(GridPlacementSystem.Instance.gridLayout.WorldToCell(item.transform.position - item.placementOffset));
//            return cells;
//        }

//        Bounds area = collider.bounds;
//        Vector3Int minCell = GridPlacementSystem.Instance.gridLayout.WorldToCell(area.min);
//        Vector3Int maxCell = GridPlacementSystem.Instance.gridLayout.WorldToCell(area.max);

//        for (int x = minCell.x; x <= maxCell.x; x++)
//        {
//            for (int y = minCell.y; y <= maxCell.y; y++)
//            {
//                cells.Add(new Vector3Int(x, y, 0));
//            }
//        }
//        return cells;
//    }
//}