using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;


// also for cursor and movement improvements https://www.youtube.com/watch?v=n5EN2J2FxOQ
// see https://www.youtube.com/watch?v=gFpmJtO0NT4 for an overview
public class GridPlacementSystem : MonoBehaviour
{

    public static GridPlacementSystem Instance { get; private set; }

    [SerializeField] private Camera placementCamera;
    public GridLayout gridLayout;
    public Tilemap MainTilemap;
    public Tilemap TempTilemap;
    public TileBase highlightTile;


    public static Dictionary<TileType, TileBase> tileBases = new Dictionary<TileType, TileBase>();

    private GridItem gridItem;
    private Vector3 previousPosition;

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
        Instance = this;
        MainTilemap.gameObject.SetActive(false);
        TempTilemap.gameObject.SetActive(false);

    }

    private void Start()
    {
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

    public GridItem InitializeWithItem(GameObject item)
    {
        gridItem = Instantiate(item, Vector3.zero, Quaternion.identity).GetComponent<GridItem>();
        //FollowItem(item);
        GameEvents.ItemSelectionPanelOpened();
        return gridItem;
    }
    private void ClearArea(BoundsInt areaToClear)
    {
        TileBase[] toClear = new TileBase[areaToClear.size.x * areaToClear.size.y * areaToClear.size.z];
        FillTiles(toClear, TileType.Empty);
        TempTilemap.SetTilesBlock(areaToClear, toClear);
    }

    public void FollowItem(GridItem gridItem)
    {
        EnableMainTilemap();
        if (_lastAreas.TryGetValue(gridItem, out var oldArea))
        {
            ClearArea(oldArea);
        }

        gridItem.area.position = gridLayout.WorldToCell(gridItem.gameObject.transform.position) - gridItem.getAdjustPosition(); // adjust to center the item correctly
        BoundsInt buildingArea = gridItem.area;

        TileBase[] baseArray = GetTilesBlock(buildingArea, MainTilemap);

        int size = baseArray.Length;
        TileBase[] tileArray = new TileBase[size];

        if (gridItem.requiresSupport)
        {
            gridItem.ShowPlacementFeedback(HasValidSupport(gridItem,buildingArea));
            gridItem.SupportedItemCanBePlaced = HasValidSupport(gridItem, buildingArea);
        }
        else
        {
            for (int i = 0; i < baseArray.Length; i++)
            {

                // this is to show where we can place with blue and if we cannot then with red:
                if (baseArray[i] == tileBases[TileType.White])
                {
                    gridItem.ShowPlacementFeedback(true);
                    //tileArray[i] = tileBases[TileType.Blue];
                }
                else
                {
                    gridItem.ShowPlacementFeedback(false);
                    //FillTiles(tileArray, TileType.Red);
                    break;
                }



            }
        }
        TempTilemap.SetTilesBlock(buildingArea, tileArray);
        _lastAreas[gridItem] = buildingArea; 
    }


    private bool HasValidSupport(GridItem gridItem, BoundsInt area)
    {
        // Check each of the four directions around the item
        foreach (var dir in new Vector3Int[] { Vector3Int.down, Vector3Int.up, Vector3Int.left, Vector3Int.right })
        {
            // Shift the area by one cell in the current direction
            BoundsInt shiftedArea = new BoundsInt(area.position + dir, area.size);

            // Get all tiles in that shifted area from the main tilemap
            TileBase[] baseArray = GetTilesBlock(shiftedArea, MainTilemap);

            foreach (var b in baseArray)
            {
                // skip empty cells
                if (b == null)
                    continue;
                // compare by reference OR name (in case of duplicated tile assets)
                if (b == tileBases[TileType.Red])
                {
                    Debug.Log($"Found valid support tile '{b.name}' in direction {dir}");
                    return true;
                }
             
            }
        }

        // No valid adjacent support found
        return false;
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

    public void HighlightCell(Vector3 worldPos)
    {
        Vector3Int cell = gridLayout.WorldToCell(worldPos);
        TempTilemap.SetTile(cell, highlightTile);
    }


    public void ClearCellHighlight(Vector3 worldPos)
    {
        Vector3Int cell = gridLayout.WorldToCell(worldPos);
        TempTilemap.SetTile(cell, null);
    }
    public void OccupyCellsMainTilemap(IEnumerable<Vector3Int> cells, TileType type)
    {
        if (!MainTilemap.gameObject.activeSelf)
            MainTilemap.gameObject.SetActive(true);

        foreach (var cell in cells)
        {

            MainTilemap.SetTile(cell, tileBases[type]);
        }
        MainTilemap.gameObject.SetActive(false);
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
