using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;


public class GridPlacementSystem : MonoBehaviour
{

    public static GridPlacementSystem Instance { get; private set; }

    [SerializeField] private Camera placementCamera;
    public GridLayout gridLayout;
    public Tilemap MainTilemap;
    public Tilemap TempTilemap;
    public Tilemap OriginalTilemap;
    
    public static Dictionary<TileType, TileBase> tileBases = new Dictionary<TileType, TileBase>();

    // track each Cursor/GridItem's last highlight area
    private Dictionary<GridItem, BoundsInt> _lastAreas = new();


    #region Unity Methods
    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }

        return array;
    }

    private static void FillTiles(TileBase[] arr, TileType type)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = tileBases[type];
        }
    }

    private static void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap)
    {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }


    private void Awake()
    {
        ResetMainTileMap();
        Instance = this;
        MainTilemap.gameObject.SetActive(false);
        TempTilemap.gameObject.SetActive(false);
        GameEvents.OnMenuStateEntered += ResetMainTileMap;

    }

    private void OnDestroy()
    {
        GameEvents.OnMenuStateEntered -= ResetMainTileMap;
    }

    private void Start()
    {
        if (tileBases == null) tileBases = new Dictionary<TileType, TileBase>();

        string tilePath = "Tiles/";
        tileBases.Add(TileType.Empty, null);
        tileBases.Add(TileType.White, Resources.Load<TileBase>(tilePath + "TilesGrid_1"));
        tileBases.Add(TileType.Red, Resources.Load<TileBase>(tilePath + "TilesGrid 1_6"));
        tileBases.Add(TileType.Turquois, Resources.Load<TileBase>(tilePath + "TilesGrid 1_0"));
        tileBases.Add(TileType.Violet, Resources.Load<TileBase>(tilePath + "TilesGrid 1_7"));
        tileBases.Add(TileType.Yellow, Resources.Load<TileBase>(tilePath + "TilesGrid 1_3"));
        tileBases.Add(TileType.Black, Resources.Load<TileBase>(tilePath + "TilesGrid 1_2"));
        tileBases.Add(TileType.Orange, Resources.Load<TileBase>(tilePath + "TilesGrid 1_1"));
        tileBases.Add(TileType.Blue, Resources.Load<TileBase>(tilePath + "TilesGrid 1_8"));


    }
    #endregion


    #region Tilemap Placement

    #endregion


    #region Item Placement

    private void ClearArea(BoundsInt areaToClear)
    {
        TileBase[] toClear = new TileBase[areaToClear.size.x * areaToClear.size.y * areaToClear.size.z];
        FillTiles(toClear, TileType.Empty);
        TempTilemap.SetTilesBlock(areaToClear, toClear);
    }

    public void FollowItem(GridItem gridItem)
    {
        if(gridItem.GetIsBomb())
        {
            return;
        }
        
        if (_lastAreas.TryGetValue(gridItem, out var oldArea))
        {
            ClearArea(oldArea);
        }
        /*
        gridItem.area.position = gridLayout.WorldToCell(gridItem.gameObject.transform.position) - gridItem.getAdjustPosition(); // adjust to center the item correctly
        BoundsInt buildingArea = gridItem.area;

        TileBase[] baseArray = GetTilesBlock(buildingArea, MainTilemap);

        int size = baseArray.Length;
        TileBase[] tileArray = new TileBase[size];
        */

        // Get all occupied cells for the item's current collider shape
        List<Vector3Int> shapeCells = gridItem.GetOccupiedCells();


        if (gridItem.isAttachable)
        {
            gridItem.ShowPlacementFeedback(HasValidSupport(gridItem));
            gridItem.SupportedItemCanBePlaced = HasValidSupport(gridItem);
        }
        else
        {

            // Determine if all cells are placeable
            bool canPlaceAll = true;
            foreach (var cell in shapeCells)
            {
                if (!CanTakeCell(cell))
                {
                    canPlaceAll = false;
                    break;
                }
            }

            // Show feedback on the sprite itself
            gridItem.ShowPlacementFeedback(canPlaceAll);
            /*
            // Draw highlight tiles (per cell)
            TempTilemap.ClearAllTiles();
            TileType type = canPlaceAll ? TileType.Blue : TileType.Red;
            foreach (var cell in shapeCells)
            {
                TempTilemap.SetTile(cell, tileBases[type]);
            }
            TempTilemap.SetTilesBlock(buildingArea, tileArray);
            _lastAreas[gridItem] = buildingArea; */
        }
    }

    // Supported items need their collisionshape where the gridcells should be white,
    // which is usually above the item, where it should also interact with players
    private bool HasValidSupport(GridItem gridItem)
    {
        bool validTiles = true;
        bool objectSupport = false;

        // --- Get shape and grid info ---
        List<Vector3Int> shapeCells = gridItem.GetOccupiedCells();
        GridLayout grid = gridLayout;

        // --- Determine facing direction ---
        Vector2 rayDir = Vector2.zero;

        switch (gridItem.currentFacingDirection)
        {
            case GridItem.FacingDirection.Up:
                rayDir = Vector2.down;
                break;
            case GridItem.FacingDirection.Down:
                rayDir = Vector2.up;
                break;
            case GridItem.FacingDirection.Left:
                rayDir = Vector2.right;
                break;
            case GridItem.FacingDirection.Right:
                rayDir = Vector2.left;
                break;
        }

        // --- STEP 1️⃣: Check that all occupied cells are WHITE ---
        foreach (var cell in shapeCells)
        {
            TileBase t = MainTilemap.GetTile(cell);
            if (t != tileBases[TileType.White])
            {
                validTiles = false;
                break;
            }
        }
        // If any tile isn't white → cannot place here
        if (!validTiles)
            return false;


        float snapDistance = 0.9f;

        List<GameObject> hitObjects = new List<GameObject>();

        for (int i = 0; i < shapeCells.Count; i++)
        {
            Vector3 worldOrigin = grid.CellToWorld(shapeCells[i]) + grid.cellSize / 2f;

            RaycastHit2D[] hits = Physics2D.RaycastAll(worldOrigin, rayDir, snapDistance);
            Color rayColor = Color.yellow;
            Debug.DrawRay(worldOrigin, rayDir * snapDistance, rayColor, snapDistance);

            if (hits.Length == 0)
                continue;

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.collider.gameObject == gridItem.gameObject) continue;

                GameObject hitGO = hit.collider.gameObject;
                string layerName = LayerMask.LayerToName(hitGO.layer);

                if (layerName == "Ground/Wall")
                {
                    hitObjects.Add(hitGO);
                    break; // only need the first valid hit
                }
            }
        }

        // --- Validate that all rays hit the SAME Ground/Wall object ---
        if (hitObjects.Count == shapeCells.Count && hitObjects.Count > 0)
        {
            GameObject firstHit = hitObjects[0];
            string firstLayer = LayerMask.LayerToName(firstHit.layer);

            bool allSame = firstLayer == "Ground/Wall" &&
                           hitObjects.TrueForAll(obj => obj == firstHit);

            if (allSame)
            {
                gridItem.ifAttachableAttachHere = firstHit.transform;
                objectSupport = true;
                // Debug.Log($"✅ All rays hit the same Ground/Wall object: {firstHit.name}");
            }
        }

        return validTiles && objectSupport;
    }



    public bool CanTakeArea(BoundsInt area)
    {
        TileBase[] baseArray = GetTilesBlock(area, MainTilemap);

        foreach (var b in baseArray)
        {
            if(b != tileBases[TileType.White])
            {
                Debug.Log("Cannot place here");
                return false;
            }
        }

        return true;
    }

    public void TakeArea(BoundsInt area)
    {
        SetTilesBlock(area, TileType.Empty, TempTilemap);
        SetTilesBlock(area, TileType.Blue, MainTilemap);
    }



    #endregion


    public void ShowGrid()
    {
        TempTilemap.ClearAllTiles();
        TempTilemap.gameObject.SetActive(true);
        //MainTilemap.gameObject.SetActive(true);

    }

    public void HideGrid()
    {
        TempTilemap.ClearAllTiles();
        TempTilemap.gameObject.SetActive(false);
        MainTilemap.gameObject.SetActive(false);

    }

    public void OccupyCellsMainTilemap(IEnumerable<Vector3Int> cells, TileType type)
    {
        //if (!MainTilemap.gameObject.activeSelf)
         //   MainTilemap.gameObject.SetActive(true);

        foreach (var cell in cells)
        {

            MainTilemap.SetTile(cell, tileBases[type]);
        }
        //MainTilemap.gameObject.SetActive(false);
    }

    public void HighlightCells(IEnumerable<Vector3Int> cells, TileType type)
    {
        foreach (var cell in cells)
        {
            TempTilemap.SetTile(cell, tileBases[type]);
        }
    }

    public void EnableMainTilemap()
    {

        MainTilemap.gameObject.SetActive(true);

        // If you just want to hide it visually:
        //var renderer = MainTilemap.GetComponent<TilemapRenderer>();
        //if (renderer != null)
        //    renderer.enabled = false;


    }

    public bool CanTakeCell(Vector3Int cell)
    {
        TileBase tile = MainTilemap.GetTile(cell);
        return (tile == tileBases[TileType.White]);
    }
    

    public void TakeCell(Vector3Int cell)
    {
        //if (!MainTilemap.gameObject.activeSelf)
        //    MainTilemap.gameObject.SetActive(true);

        // Mark this cell as occupied (blue tile)
        MainTilemap.SetTile(cell, tileBases[TileType.Red]);
    }
    
    public void ClearCell(Vector3Int cell)
    {
        //if (!MainTilemap.gameObject.activeSelf)
        //    MainTilemap.gameObject.SetActive(true);

        // Mark this cell as occupied (blue tile)
        MainTilemap.SetTile(cell, tileBases[TileType.White]);
    }
    
    public void ResetMainTileMap()
    {
        CopyTilemap(OriginalTilemap, MainTilemap);
    }

    private void CopyTilemap(Tilemap source, Tilemap target)
    {
        // Clear target first
        target.ClearAllTiles();

        // Use the bounds/int position range of the source tilemap
        BoundsInt bounds = source.cellBounds;
        TileBase[] tiles = source.GetTilesBlock(bounds);
        target.SetTilesBlock(bounds, tiles);
    }
}





public enum TileType
{

    Empty,
    White,
    Blue,
    Red,
    Turquois,
    Green,
    Orange,
    Violet,
    Yellow,
    Black
    
}
